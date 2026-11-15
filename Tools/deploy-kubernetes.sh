#!/usr/bin/env bash
set -euo pipefail
if [[ $# -ne 3 ]]; then
  echo "Usage: deploy-kubernetes.sh API_IMAGE NOTIFICATION_IMAGE FRONTEND_IMAGE" >&2
  exit 2
fi
repo_root="$(cd "$(dirname "$0")/.." && pwd)"
namespace=travel-agency
# Infrastructure and credentials are provisioned separately. A code deploy never
# replaces SQL storage, initializes databases, or installs example credentials.
kubectl get namespace "$namespace" >/dev/null
kubectl get secret travel-agency-secrets -n "$namespace" >/dev/null
kubectl get configmap travel-agency-config -n "$namespace" >/dev/null
staging_dir="$(mktemp -d)"
trap 'rm -rf "$staging_dir"' EXIT
cp "$repo_root/Infrastructure/kubernetes/api-deployment.yaml" "$staging_dir/api.yaml"
cp "$repo_root/Infrastructure/kubernetes/notification-service-deployment.yaml" "$staging_dir/notification.yaml"
cp "$repo_root/Infrastructure/kubernetes/frontend.yaml" "$staging_dir/frontend.yaml"
cp "$repo_root/Infrastructure/nginx/frontend.conf" "$staging_dir/frontend.conf"
cat > "$staging_dir/kustomization.yaml" <<EOF
apiVersion: kustomize.config.k8s.io/v1beta1
kind: Kustomization
namespace: $namespace
resources:
  - api.yaml
  - notification.yaml
  - frontend.yaml
configMapGenerator:
  - name: travel-agency-frontend-nginx
    files:
      - frontend.conf
images:
  - name: travel-agency-api
    newName: ${1%:*}
    newTag: ${1##*:}
  - name: travel-agency-notification-service
    newName: ${2%:*}
    newTag: ${2##*:}
  - name: travel-agency-frontend
    newName: ${3%:*}
    newTag: ${3##*:}
EOF
kubectl apply -k "$staging_dir"
for deployment in travel-agency-api notification-service travel-agency-frontend; do
  if ! kubectl rollout status "deployment/$deployment" -n "$namespace" --timeout=240s; then
    echo "Rollout failed. Review kubectl describe/logs; the deployment retains its rollout history." >&2
    exit 1
  fi
done
kubectl port-forward -n "$namespace" service/travel-agency-frontend 18080:80 > "$staging_dir/forward.log" 2>&1 &
forward_pid=$!
trap 'kill "$forward_pid" 2>/dev/null || true; rm -rf "$staging_dir"' EXIT
curl --fail --retry 10 --retry-connrefused --retry-delay 2 http://127.0.0.1:18080/ >/dev/null
curl --fail --retry 5 --retry-delay 2 http://127.0.0.1:18080/api/v1/packages >/dev/null
echo "Staging rollout and database-backed smoke request passed."

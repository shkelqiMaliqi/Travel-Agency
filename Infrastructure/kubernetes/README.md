# Travel Agency Kubernetes Manifests

These manifests provide a production-style Kubernetes deployment shape for the Travel Agency project.

## What Is Included

- Namespace: `travel-agency`
- ASP.NET Core API deployment with 2 replicas
- API `ClusterIP` service
- HorizontalPodAutoscaler for API pods
- Notification microservice deployment
- Redis deployment/service
- RabbitMQ deployment/service
- SQL Server deployment/service with persistent data and backup volumes
- SQL Server initialization job using `db-init/Travel_AgencyDB.sql`
- MongoDB deployment/service for analytics
- MinIO deployment/service for S3-compatible storage
- NGINX ingress resource
- Nightly SQL Server backup CronJob

## Local Docker Desktop Kubernetes Notes

Kubernetes must be enabled in Docker Desktop before these commands can run.

Build the local images first:

```powershell
docker build -t travel-agency-api:latest .
docker build -t travel-agency-notification-service:latest -f Microservices/NotificationService/Dockerfile .
```

Render the manifests without applying:

```powershell
kubectl kustomize Infrastructure/kubernetes
```

Apply the manifests:

```powershell
kubectl apply -k Infrastructure/kubernetes
```

Check status:

```powershell
kubectl get all -n travel-agency
kubectl get ingress -n travel-agency
kubectl get jobs -n travel-agency
```

## Requirements For Full Runtime

- A running Kubernetes cluster.
- Local images built and available to the cluster, or pushed to a registry.
- An ingress controller such as NGINX Ingress.
- Metrics Server for the HPA CPU metrics.
- Real secrets for production use.

## Local Add-Ons Used For Testing

Metrics Server was installed so the HorizontalPodAutoscaler can read real CPU usage:

```powershell
kubectl apply -f https://github.com/kubernetes-sigs/metrics-server/releases/download/v0.8.1/components.yaml
```

Docker Desktop Kubernetes may need this extra argument because the local kubelet certificate does not include the node IP:

```powershell
kubectl get deployment metrics-server -n kube-system -o json
# Add --kubelet-insecure-tls to the metrics-server container args if needed.
```

NGINX Ingress Controller was installed so the `travel-agency.local` ingress can route to the API:

```powershell
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.15.1/deploy/static/provider/cloud/deploy.yaml
```

If Windows cannot resolve `travel-agency.local`, add this line to `C:\Windows\System32\drivers\etc\hosts` using Administrator permissions:

```text
127.0.0.1 travel-agency.local
```

Without editing the hosts file, the ingress can still be tested with a Host header:

```powershell
Invoke-WebRequest http://localhost/health -Headers @{ Host = 'travel-agency.local' }
```

## Honest Status

The manifests render and apply successfully when Kubernetes is enabled in Docker Desktop. On the tested machine, the Travel Agency pods, services, SQL initialization job, Metrics Server, and NGINX Ingress Controller were verified successfully.

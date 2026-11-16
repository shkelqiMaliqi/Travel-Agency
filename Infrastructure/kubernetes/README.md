# Kubernetes deployment

Read `Documentation/DEPLOYMENT.md` from the repository root. Use `kubectl kustomize Infrastructure/kubernetes` to render the base without applying it. Example secrets and the example TLS ingress are deliberately excluded from the base.

For a local Docker Desktop/kind/minikube cluster, generate `.env`, build/load the three application images tagged `:local`, run `Tools/start-kubernetes-local.ps1`, then run `Tools/deploy-kubernetes.sh` with those three image names. The initializer checks the context, preserves existing secrets/data, and initializes SQL only if absent. Mailpit provides a local email inbox; it is not an external email provider.

For a published environment, provision real credentials, trusted ingress addresses, SMTP, and TLS separately. Apply the TLS ingress only after substituting its domain and installing its certificate. HPA needs Metrics Server. The deployment pipeline performs a disposable kind rollout when dispatched; inspect actual run artifacts before claiming successful execution.

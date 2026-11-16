# Deployment and environment boundaries

The native Windows environment is verified separately from Docker/Kubernetes. A successful manifest render is not a successful rollout. A local CI definition is not evidence of a GitHub-hosted execution.

## Local infrastructure prerequisites

Install Docker Desktop from its official vendor, enable WSL2/Linux containers and a local Kubernetes cluster (Docker Desktop, kind, or minikube). Installing/enabling WSL may need administrator rights and a Windows restart. No automatic reboot is performed by this project. Install kubectl and verify `docker version` and `kubectl cluster-info` before running deployment scripts.

Run `Tools/initialize-local.ps1` once. It creates ignored `.env` values and refuses to overwrite incomplete existing configuration. If copying `.env.example`, fill its values explicitly; remove only an unused empty example to generate new local values. Never rotate the SQL password of an existing database volume by deleting `.env`.

## Local Kubernetes demonstration

1. Build images with `docker build -t travel-agency-api:local .`, `docker build -t travel-agency-notification-service:local -f Microservices/NotificationService/Dockerfile .`, and `docker build -t travel-agency-frontend:local -f UI/travel-agency/Dockerfile .`.
2. For kind, use cluster name `travel-agency-ci`, then `kind load docker-image --name travel-agency-ci travel-agency-api:local travel-agency-notification-service:local travel-agency-frontend:local`. Docker Desktop can use its local images directly.
3. Run `Tools/start-kubernetes-local.ps1`. It creates the local namespace and random private secrets if absent, applies the bundle, and waits for the SQL initialization job. Existing database and secrets are preserved.
4. Run `bash Tools/deploy-kubernetes.sh travel-agency-api:local travel-agency-notification-service:local travel-agency-frontend:local`. The script waits for three rollouts and checks frontend/database-backed HTTP requests. It does not initialize storage or rotate secrets.
5. Show `kubectl get pods,services,jobs -n travel-agency`, then port-forward the frontend service to 3101 and Mailpit to 8025 for the demonstration. HPA additionally requires Metrics Server. The Compose stack demonstrates Prometheus/Grafana and explicit two-upstream NGINX balancing.

## Published environment

Provision SQL/cache/queue/storage and private configuration separately. Provide a unique random `Jwt__Secret`, database connection, RabbitMQ credentials, SMTP credentials, and storage credentials using a secret manager/Kubernetes Secret. Do not use seeded demo user passwords or the local Mailpit deployment for public account email.

Configure SMTP with the six `Email__Smtp__*` settings shown in the README/.env example and verify delivery to account owners. Configure trusted ingress IPs using `ReverseProxy__KnownProxies` (comma-separated exact addresses). Arbitrary forwarded headers are not trusted. Configure ingress/frontends to pass the original scheme consistently.

Replace `travel-agency.example.com` in the separate `Infrastructure/kubernetes/ingress.yaml`, create the `travel-agency-tls` certificate Secret through your certificate provider, and apply the ingress after the controller is installed. Its route serves the frontend, which proxies `/api/` to the API. It enforces HTTPS. The base bundle intentionally does not apply an example ingress/certificate or example Secret automatically. Establish backups and perform a restore drill before depending on the deployment.

## Advanced pipeline

Dispatch `Verify infrastructure and deploy staging` with publication/deployment unchecked to run integration and Kubernetes verification. The Compose job verifies SQL-backed tests, local email, cross-replica cache invalidation, RabbitMQ delivery, metrics, and production serving. A separate job creates kind and executes the rollout script.

To publish, opt into `publish`; the pipeline uses the repository's scoped GitHub token and tags all three images with the commit. To deploy, enable both inputs and provision the `staging` GitHub environment with a base64 kubeconfig in `KUBE_CONFIG_BASE64`. Configure required reviewers in GitHub if desired. The kubeconfig must target the intended environment and have only necessary access. The cluster needs GHCR pull access for private images. The deploy job updates only application resources and fails on a rollout/smoke-check failure.

The local repair did not publish images or contact a remote cluster. Evidence from an actual workflow run must be attached after execution.

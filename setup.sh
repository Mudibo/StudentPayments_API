#!/bin/bash
set -e

# --- Step 1: Ensure prerequisites ---
# Docker Desktop must be running with Kubernetes enabled.
# kubectl must be installed and configured (Docker Desktop bundles it).
kubectl apply -f k8s/namespace.yaml


# --- Step 3: Run database migration Job ---

# --- Step 4: Apply Kubernetes manifests ---
echo "Applying Kubernetes manifests..."
kubectl apply -f k8s/ --recursive

 # --- Step 4: Wait for all pods to be ready ---
echo "Waiting for all pods to be ready in the 'studentpayments' namespace..."
kubectl wait --for=condition=Ready pod --all -n studentpayments --timeout=120s

 # --- Step 5: Show current services ---
echo "Current services in 'studentpayments' namespace:"
kubectl get svc -n studentpayments

 # --- Step 6: Port-forward nginx service for local access ---

echo "Port-forwarding nginx service to localhost:8080..."
echo "You can access the app at http://localhost:8080"
kubectl port-forward svc/nginx 8080:8080 -n studentpayments &
PORT_FORWARD_PID=$!

# Wait a few seconds for port-forward to establish
sleep 5

# Open Swagger UI in the default browser (Linux/macOS/WSL/Windows)
SWAGGER_URL="http://localhost:8080/swagger"
if command -v xdg-open > /dev/null; then
	xdg-open "$SWAGGER_URL"
elif command -v open > /dev/null; then
	open "$SWAGGER_URL"
elif command -v start > /dev/null; then
	start "$SWAGGER_URL"
else
	echo "Please open $SWAGGER_URL in your browser."
fi


# --- Step 7: Print pgAdmin NodePort and open pgAdmin in browser ---
echo "\nChecking pgAdmin NodePort..."
PGADMIN_NODEPORT=$(kubectl get svc pgadmin -n studentpayments -o jsonpath='{.spec.ports[0].nodePort}')
if [ -n "$PGADMIN_NODEPORT" ]; then
	PGADMIN_URL="http://localhost:$PGADMIN_NODEPORT"
	echo "You can access pgAdmin at $PGADMIN_URL"
	# Try to open pgAdmin in the default browser
	if command -v xdg-open > /dev/null; then
		xdg-open "$PGADMIN_URL"
	elif command -v open > /dev/null; then
		open "$PGADMIN_URL"
	elif command -v start > /dev/null; then
		start "$PGADMIN_URL"
	else
		echo "Please open $PGADMIN_URL in your browser."
	fi
else
	echo "Could not determine pgAdmin NodePort. Please check the service manually."
fi

# Wait for port-forward to finish (user can Ctrl+C to stop)
wait $PORT_FORWARD_PID


 # --- Step 7: (Optional) Monitor pods and logs ---
# To check pod status: kubectl get pods -n studentpayments
# To view logs: kubectl logs <pod-name> -n studentpayments

# --- Step 8: (Optional) Clean up resources ---
# To delete all resources: kubectl delete -f k8s/
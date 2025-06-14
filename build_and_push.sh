#!/bin/bash
set -e

# Docker Hub user/repo prefix
REPO="paulschein/sound-case-opener"

echo "Building and pushing migrations..."
docker build -f backend/Dockerfile.Migrations -t $REPO:migrations-latest backend
docker push $REPO:migrations-latest

echo "Building and pushing backend..."
docker build -f backend/Dockerfile -t $REPO:backend-latest backend
docker push $REPO:backend-latest

echo "Building and pushing frontend..."
docker build -f frontend/Dockerfile -t $REPO:frontend-latest frontend
docker push $REPO:frontend-latest

echo "All images built and pushed successfully."
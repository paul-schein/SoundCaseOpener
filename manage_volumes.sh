#!/bin/bash

# Change volume prefix as needed
PREFIX="sound_case_opener"

DB_VOLUME="$PREFIX-db_data"
SOUNDS_VOLUME="$PREFIX-sounds"

usage() {
  echo "Usage: $0 [create|remove]"
  exit 1
}

if [ "$#" -ne 1 ]; then
  usage
fi

case "$1" in
  create)
    echo "Creating volumes..."
    docker volume create "$DB_VOLUME"
    docker volume create "$SOUNDS_VOLUME"
    echo "Volumes created."
    ;;
  remove)
    echo "Removing volumes..."
    docker volume rm "$DB_VOLUME"
    docker volume rm "$IMAGES_VOLUME"
    echo "Volumes removed."
    ;;
  *)
    usage
    ;;
esac

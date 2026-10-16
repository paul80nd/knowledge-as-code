#!/bin/sh
# Writes a small Go HTTP service with no Dockerfile.
set -eu

cat > go.mod <<'EOF'
module example.invalid/stocklevels

go 1.23
EOF

cat > main.go <<'EOF'
package main

import (
	"encoding/json"
	"log"
	"net/http"
)

var levels = map[string]int{"SKU-1001": 12, "SKU-1002": 0}

func main() {
	http.HandleFunc("/v1/levels", func(w http.ResponseWriter, r *http.Request) {
		w.Header().Set("Content-Type", "application/json")
		_ = json.NewEncoder(w).Encode(levels)
	})
	log.Fatal(http.ListenAndServe(":8080", nil))
}
EOF

cat > README.md <<'EOF'
# stocklevels

Publishes stock levels on port 8080.

    go run .
EOF

git init -q
git add .
git -c user.name=fixture -c user.email=fixture@example.invalid commit -q -m "Start the stocklevels service"

#!/bin/sh
# Writes a small Go HTTP service with a test and no CI.
set -eu

cat > go.mod <<'EOF'
module example.invalid/quotes

go 1.23
EOF

cat > main.go <<'EOF'
package main

import (
	"log"
	"net/http"
)

func main() {
	http.HandleFunc("/v1/quote", quoteHandler)
	log.Fatal(http.ListenAndServe(":8080", nil))
}
EOF

cat > quote.go <<'EOF'
package main

import (
	"fmt"
	"net/http"
	"strconv"
)

func shippingPence(grams int) int {
	if grams <= 1000 {
		return 395
	}
	return 395 + (grams-1000+499)/500*150
}

func quoteHandler(w http.ResponseWriter, r *http.Request) {
	grams, err := strconv.Atoi(r.URL.Query().Get("grams"))
	if err != nil || grams < 0 {
		http.Error(w, "grams must be a whole number", http.StatusBadRequest)
		return
	}
	fmt.Fprintf(w, "{\"pence\":%d}", shippingPence(grams))
}
EOF

cat > quote_test.go <<'EOF'
package main

import "testing"

func TestShippingPenceForOneKilo(t *testing.T) {
	if got := shippingPence(1000); got != 395 {
		t.Fatalf("got %d", got)
	}
}
EOF

cat > README.md <<'EOF'
# quotes

Quotes a shipping price by weight.

    go test ./...
    go run .
EOF

git init -q
git add .
git -c user.name=fixture -c user.email=fixture@example.invalid commit -q -m "Start the quotes service"

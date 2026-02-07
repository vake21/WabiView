# WabiView Makefile for StartOS packaging

PACKAGE_ID := wabi-view
VERSION := 0.1.0

# StartOS SDK paths (adjust if needed)
START_SDK := start-sdk

.PHONY: all clean build package verify

all: package

# Clean build artifacts
clean:
	rm -rf src/WabiView/bin src/WabiView/obj
	rm -f $(PACKAGE_ID).s9pk

# Build the Docker image
build:
	docker build -t $(PACKAGE_ID):$(VERSION) -f startos/Dockerfile .

# Package for StartOS
package: build
	@echo "Packaging $(PACKAGE_ID) v$(VERSION) for StartOS..."
	cd startos && $(START_SDK) pack

# Verify the package
verify:
	$(START_SDK) verify $(PACKAGE_ID).s9pk

# Development helpers
.PHONY: dev run logs

# Run locally with docker-compose
dev:
	cd startos && docker-compose up --build

# Run detached
run:
	cd startos && docker-compose up -d --build

# View logs
logs:
	cd startos && docker-compose logs -f

# Stop local dev
stop:
	cd startos && docker-compose down

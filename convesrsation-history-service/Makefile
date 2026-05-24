.PHONY: help build test run clean restore format lint validate docker-build docker-run docker-clean

# Variables
PROJECT_NAME := ConversationService
DOTNET_VERSION := 8.0
IMAGE_NAME := conversationservice
DOCKER_REGISTRY := ghcr.io
DOCKER_REPO := $(DOCKER_REGISTRY)/$(DOCKER_USER)/$(IMAGE_NAME)

# Couleurs
BLUE := \033[0;34m
GREEN := \033[0;32m
RED := \033[0;31m
YELLOW := \033[0;33m
NC := \033[0m

help: ## Affiche cette aide
	@echo "$(BLUE)==== $(PROJECT_NAME) Makefile ===="
	@echo "$(NC)"
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "$(GREEN)%-20s$(NC) %s\n", $$1, $$2}'
	@echo ""

# ─── Développement ──────────────────────────────────────────────

restore: ## Restaurer les dépendances NuGet
	@echo "$(BLUE)Restoring dependencies...$(NC)"
	@dotnet restore

build: ## Build le projet en mode Release
	@echo "$(BLUE)Building project...$(NC)"
	@dotnet build --configuration Release --no-restore

build-debug: ## Build le projet en mode Debug
	@echo "$(BLUE)Building project (Debug)...$(NC)"
	@dotnet build --configuration Debug --no-restore

clean: ## Nettoyer les artifacts (bin, obj)
	@echo "$(BLUE)Cleaning artifacts...$(NC)"
	@dotnet clean
	@rm -rf bin obj
	@echo "$(GREEN)✓ Cleaned$(NC)"

# ─── Tests ──────────────────────────────────────────────────────

test: ## Exécuter les tests unitaires
	@echo "$(BLUE)Running unit tests...$(NC)"
	@dotnet test --configuration Release --no-restore --logger "console;verbosity=minimal"

test-verbose: ## Exécuter les tests avec détails
	@echo "$(BLUE)Running unit tests (verbose)...$(NC)"
	@dotnet test --configuration Release --no-restore --logger "console;verbosity=normal"

test-coverage: ## Exécuter les tests avec couverture de code
	@echo "$(BLUE)Running unit tests with coverage...$(NC)"
	@dotnet test --configuration Release --no-restore \
		/p:CollectCoverage=true \
		/p:CoverageFormat=opencover \
		--logger "console;verbosity=minimal"
	@echo "$(GREEN)✓ Coverage report: coverage.opencover.xml$(NC)"

test-watch: ## Exécuter les tests en mode watch
	@echo "$(BLUE)Running tests in watch mode...$(NC)"
	@dotnet watch --project Tests/ConversationHistoryService.Tests.csproj test

# ─── Code Quality ───────────────────────────────────────────────

format: ## Formater le code selon les règles EditorConfig
	@echo "$(BLUE)Formatting code...$(NC)"
	@dotnet format
	@echo "$(GREEN)✓ Code formatted$(NC)"

format-check: ## Vérifier le formatage sans modifier
	@echo "$(BLUE)Checking code formatting...$(NC)"
	@dotnet format --verify-no-changes --verbosity quiet || \
		(echo "$(RED)✗ Formatting issues found. Run: make format$(NC)" && exit 1)

lint: ## Vérifier les dépendances vulnerables
	@echo "$(BLUE)Checking vulnerable packages...$(NC)"
	@dotnet list package --vulnerable || true

# ─── Validation Pipeline ────────────────────────────────────────

validate: ## Valider le pipeline localement (complet)
	@echo "$(BLUE)Validating pipeline locally...$(NC)"
	@powershell -Command "if ($$PSVersionTable.PSVersion.Major -ge 5) { .\.github\scripts\validate-pipeline.ps1 } else { bash ./.github/scripts/validate-pipeline.sh }"

validate-fast: ## Valider rapidement (sans Docker)
	@echo "$(BLUE)Quick validation...$(NC)"
	@powershell -Command "if ($$PSVersionTable.PSVersion.Major -ge 5) { .\.github\scripts\validate-pipeline.ps1 -SkipDocker } else { bash ./.github/scripts/validate-pipeline.sh }"

# ─── Exécution ──────────────────────────────────────────────────

run: ## Exécuter le projet localement
	@echo "$(BLUE)Running application...$(NC)"
	@dotnet run --project ConvesrsationHistoryService.csproj

run-debug: ## Exécuter le projet en mode Debug
	@echo "$(BLUE)Running application (Debug)...$(NC)"
	@dotnet run --project ConvesrsationHistoryService.csproj --configuration Debug

watch: ## Exécuter en mode watch (recompile au changement)
	@echo "$(BLUE)Running in watch mode...$(NC)"
	@dotnet watch --project ConvesrsationHistoryService.csproj run

# ─── Docker ─────────────────────────────────────────────────────

docker-build: ## Builder l'image Docker
	@echo "$(BLUE)Building Docker image: $(IMAGE_NAME):latest...$(NC)"
	@docker build -t $(IMAGE_NAME):latest -t $(IMAGE_NAME):$$(date +%Y%m%d_%H%M%S) .
	@echo "$(GREEN)✓ Image built successfully$(NC)"

docker-build-full: ## Builder avec full tagging
	@echo "$(BLUE)Building Docker image with full tags...$(NC)"
	@docker build -t $(IMAGE_NAME):latest \
		-t $(IMAGE_NAME):$$(git rev-parse --short HEAD) \
		-t $(IMAGE_NAME):$$(date +%Y.%m.%d.%H%M) .
	@echo "$(GREEN)✓ Image built successfully$(NC)"

docker-run: ## Exécuter le conteneur Docker
	@echo "$(BLUE)Running Docker container...$(NC)"
	@docker compose up -d
	@echo "$(GREEN)✓ Container running$(NC)"

docker-logs: ## Afficher les logs Docker
	@echo "$(BLUE)Docker logs...$(NC)"
	@docker compose logs -f conversationservice

docker-stop: ## Arrêter le conteneur Docker
	@echo "$(BLUE)Stopping Docker container...$(NC)"
	@docker compose down
	@echo "$(GREEN)✓ Container stopped$(NC)"

docker-clean: ## Nettoyer les images et conteneurs
	@echo "$(BLUE)Cleaning Docker artifacts...$(NC)"
	@docker compose down -v
	@docker image prune -f --filter "dangling=true"
	@echo "$(GREEN)✓ Cleaned$(NC)"

docker-test: ## Tester l'image Docker
	@echo "$(BLUE)Testing Docker build...$(NC)"
	@docker build -t $(IMAGE_NAME):test . > /dev/null 2>&1 && \
		docker rmi $(IMAGE_NAME):test > /dev/null 2>&1 && \
		echo "$(GREEN)✓ Docker build test passed$(NC)" || \
		(echo "$(RED)✗ Docker build test failed$(NC)" && exit 1)

# ─── Database ────────────────────────────────────────────────────

migrate: ## Appliquer les migrations EF Core
	@echo "$(BLUE)Applying migrations...$(NC)"
	@dotnet ef database update

migrate-add: ## Créer une nouvelle migration
	@read -p "Enter migration name: " name; \
	dotnet ef migrations add $$name

# ─── Utility ─────────────────────────────────────────────────────

all: clean restore build test format-check ## Exécuter tous les checks (full validation)
	@echo "$(GREEN)✓ All checks passed$(NC)"

quick: restore build test ## Quick build & test
	@echo "$(GREEN)✓ Quick validation complete$(NC)"

health: ## Tester l'endpoint santé
	@echo "$(BLUE)Testing health endpoint...$(NC)"
	@curl -s http://localhost:8088/health | jq . || echo "Health endpoint unreachable"

deps: ## Lister les dépendances
	@echo "$(BLUE)NuGet Dependencies:$(NC)"
	@dotnet list package

update-deps: ## Mettre à jour les dépendances NuGet
	@echo "$(BLUE)Updating NuGet packages...$(NC)"
	@dotnet add package --package-directory packages

info: ## Afficher les informations du projet
	@echo "$(BLUE)Project Information:$(NC)"
	@echo "  Name: $(PROJECT_NAME)"
	@echo "  .NET Version: $(DOTNET_VERSION)"
	@echo "  Docker Image: $(IMAGE_NAME)"
	@echo "  Repository: $(DOCKER_REPO)"
	@dotnet --version
	@docker --version

.DEFAULT_GOAL := help

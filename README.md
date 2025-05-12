# Magic OCR

Une app intelligente pour extraire des informations de vos documents en utilisant la puissance des modèles de vision-langage (VLM) de Mistral AI, implémentée en Blazor.

Ce projet est une adaptation en Blazor du projet Python [MagicMistralOCR](https://github.com/VincentGourbin/MagicMistralOCR).

## Fonctionnalités

* **Détection automatique de sections** : Analyse des documents pour identifier automatiquement les titres, champs et sections
* **Saisie manuelle de sections** : Ajout de sections personnalisées pour une extraction précise
* **Traitement par lots** : Analyse de plusieurs documents du même type en une seule fois
* **Format JSON structuré** : Résultats organisés et faciles à traiter pour l'intégration à d'autres systèmes
* **Mode expert** : Personnalisez les instructions d'extraction pour des cas d'utilisation spécifiques
* **Authentification Azure Entra ID** : Sécurisez votre application avec l'authentification Microsoft

## Configuration

### Variables d'environnement pour l'API Mistral

L'application s'appuie sur les variables d'environnement suivantes pour la configuration de l'API Mistral :

```csharp
var mistralApiKey = configuration["MISTRAL_API_KEY"] ??
                     throw new InvalidOperationException("MISTRAL_API_KEY is not configured");
var mistralEndpoint = configuration["MISTRAL_ENDPOINT"] ??
                      "https://ais-doc-ocr-dev.services.ai.azure.com/models";
var mistralModel = configuration["MISTRAL_MODEL"] ??
                   "mistral-small-2503";
```

### Configuration de l'authentification Azure Entra ID

#### Client

```json
{
  "AzureAd": {
    "ClientId": "",
    "Authority": "",
    "ValidateAuthority": true
  },
  "ServerApi": {
    "BaseUrl": "",
    "Scopes": ""
  }
}
```

#### API

```json
{
  "AzureAd": {
    "Instance": "",
    "Domain": "",
    "TenantId": "",
    "ClientId": "",
    "CallbackPath": "/signin-oidc",
    "Scopes": "access_as_user"
  }
}
```

## Déploiement avec Docker

L'application est disponible en tant qu'image Docker, facilitant son déploiement dans divers environnements.

### Construction de l'image Docker

```bash
# À la racine du projet
docker build -t magicmistralocr-blazor:latest .
```

### Exécution du conteneur

```bash
# Exécution basique
docker run -d -p 8080:80 --name magicmistralocr magicmistralocr-blazor:latest

# Avec variables d'environnement pour Mistral API
docker run -d -p 8080:80 \
  -e MISTRAL_API_KEY="votre-clé-api" \
  -e MISTRAL_ENDPOINT="https://votre-endpoint.services.ai.azure.com/models" \
  -e MISTRAL_MODEL="mistral-small-2503" \
  --name magicmistralocr magicmistralocr-blazor:latest
```

### Utilisation avec Docker Compose

Créez un fichier `docker-compose.yml` :

```yaml
version: '3.8'
services:
  magicmistralocr:
    image: magicmistralocr-blazor:latest
    ports:
      - "8080:80"
    environment:
      - MISTRAL_API_KEY=votre-clé-api
      - MISTRAL_ENDPOINT=https://votre-endpoint.services.ai.azure.com/models
      - MISTRAL_MODEL=mistral-small-2503
    restart: unless-stopped
```

Puis lancez avec :

```bash
docker-compose up -d
```

## Comment utiliser

### 1. Authentification

* Connectez-vous à l'application en utilisant vos identifiants Azure Entra ID.

### 2. Configurer les sections

* Téléchargez un document modèle et cliquez sur "Magic Scan" pour détecter automatiquement les sections.
* OU ajoutez manuellement des sections en les saisissant (une par ligne) et en cliquant sur "Ajouter ces sections".
* Cochez les sections que vous souhaitez extraire.

### 3. Extraire les valeurs

* Téléchargez un ou plusieurs documents du même type.
* Pour une extraction avancée, utilisez le mode expert pour personnaliser les instructions d'extraction.
* Cliquez sur "Extraire les valeurs" pour obtenir les informations des sections sélectionnées.
* Les résultats sont disponibles au format JSON.

## Différences avec la version Python

Cette version en Blazor présente les différences suivantes par rapport à la version Python originale :

* Interface utilisateur construite avec Blazor au lieu de Gradio
* Intégration avec Azure pour l'API Mistral
* Authentification via Azure Entra ID
* Architecture adaptée au framework .NET

## Prérequis

* .NET 8.0 ou supérieur
* Compte Azure avec accès à Mistral AI
* Azure Entra ID configuré pour l'authentification

## Licence

Même licence que le projet original : [MagicMistralOCR](https://github.com/VincentGourbin/MagicMistralOCR)
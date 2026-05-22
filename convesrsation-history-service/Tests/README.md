# Tests Unitaires - ConversationService

Cette suite de tests couvre toutes les fonctionnalités du service `ConversationService`.

## Structure des Tests

Les tests sont organisés par méthode du service avec des cas de test pour différents scénarios :

### GetMyConversationsAsync
- ✅ Retourne les conversations de l'utilisateur
- ✅ Retourne une liste vide si l'utilisateur n'a pas de conversations

### GetConversationAsync
- ✅ Retourne les détails d'une conversation pour le propriétaire
- ✅ Lève une exception si un utilisateur non-propriétaire tente d'accéder
- ✅ Permet l'accès administrateur même sans être propriétaire
- ✅ Lève une exception si la conversation n'existe pas
- ✅ Ordonne les messages par timestamp

### CreateConversationAsync
- ✅ Crée une conversation avec des données valides
- ✅ Enregistre l'action dans les logs

### AddMessageAsync
- ✅ Ajoute un message et met à jour le timestamp
- ✅ Lève une exception pour un utilisateur non-propriétaire
- ✅ Permet l'ajout de message pour les administrateurs
- ✅ Valide le type d'émetteur (User/AI)
- ✅ Lève une exception si la conversation n'existe pas

### DeleteConversationAsync
- ✅ Supprime une conversation pour le propriétaire
- ✅ Lève une exception pour un utilisateur non-propriétaire
- ✅ Permet la suppression pour les administrateurs
- ✅ Lève une exception si la conversation n'existe pas

### GetAllConversationsAsync
- ✅ Retourne toutes les conversations
- ✅ Retourne une liste vide si aucune conversation n'existe

## Comment Exécuter les Tests

### Depuis la ligne de commande

```powershell
# Exécuter tous les tests
dotnet test

# Exécuter les tests du projet Tests
dotnet test Tests/ConversationHistoryService.Tests.csproj

# Exécuter les tests avec un filtre
dotnet test --filter "ConversationServiceTests"

# Exécuter les tests avec verbosité détaillée
dotnet test -v d
```

### Depuis Visual Studio

1. Ouvrir l'explorateur de tests (Test > Test Explorer)
2. Les tests apparaissent dans la fenêtre de l'explorateur
3. Cliquer sur "Run All" ou sur un test spécifique

### Depuis Visual Studio Code

1. S'assurer que l'extension ".NET Test Explorer" est installée
2. Les tests apparaissent dans la vue Test Explorer
3. Cliquer sur le bouton "Run All Tests" ou sur un test spécifique

## Dépendances

- **xunit** : Framework de test
- **Moq** : Bibliothèque de mock pour les dépendances
- **FluentAssertions** : Assertions fluides pour des tests plus lisibles

## Couverture des Tests

La suite couvre :
- ✅ Tous les cas de succès
- ✅ Les contrôles d'autorisation (user vs admin)
- ✅ Les validations des entrées
- ✅ Les exceptions
- ✅ Les interactions avec les dépôts (Repository) via Moq
- ✅ Les appels au logger

## Notes

- Les tests utilisent des `Mock` pour isoler le service des dépôts réels
- Chaque test est indépendant et n'affecte pas les autres
- Les données de test sont créées dans chaque test (pas de dépendance entre les tests)

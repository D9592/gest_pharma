# Mini-pharmacie

Socle technique local pour Windows, entièrement en français, basé sur .NET 10, WinForms et SQLite.

## Licence

Le code de MiniPharmacie est distribué sous licence MIT. Voir [LICENSE.txt](LICENSE.txt). Les dépendances conservent leurs licences respectives.

## Installation et prérequis

- Windows 10/11 x64.
- SDK .NET 10 pour compiler et publier. L’exécutable autonome fourni n’exige pas l’installation du runtime .NET sur le PC cible.
- Accès à NuGet au premier `restore` (packages `Microsoft.Data.Sqlite` et xUnit).
- Un dossier accessible en écriture sous `%LOCALAPPDATA%`.

Sur le poste de développement, depuis le dossier de la solution:

```powershell
dotnet restore .\MiniPharmacie.slnx
dotnet build .\MiniPharmacie.slnx -c Release --no-restore
dotnet test .\MiniPharmacie.Tests\MiniPharmacie.Tests.csproj -c Release --no-build
dotnet run --project .\MiniPharmacie.App\MiniPharmacie.App.csproj
```

Le projet WinForms cible `net10.0-windows`; il ne peut pas s'exécuter sous Linux.

## Structure

- `MiniPharmacie.App` : interface Windows Forms et gestion des erreurs au démarrage
- `MiniPharmacie.Business` : services métier et sécurité
- `MiniPharmacie.Data` : connexions SQLite et migrations de schéma versionnées (v1 à v7)
- `MiniPharmacie.Tests` : tests xUnit

Le code métier est séparé des formulaires. Certains modules sont encore accessibles uniquement par services ou ne disposent pas encore d'un écran dédié; voir « État connu et limites » avant toute utilisation opérationnelle.

## Configuration et données

La base est créée automatiquement au premier lancement sous `%LOCALAPPDATA%\MiniPharmacie\pharmacie.db`. Elle ne contient aucune donnée de démonstration. La devise est représentée en unités mineures entières (`INTEGER`, par exemple centimes) afin d'éviter les erreurs d'arrondi des flottants. Les dates sont des chaînes ISO 8601. Les connexions activent les clés étrangères, le journal WAL est utilisé, et le schéma démarre à la version 1 dans `SchemaVersions`.

Pour toute évolution, ajouter une migration numérotée et l'appliquer dans `DatabaseInitializer` sous transaction uniquement si sa version n'est pas déjà enregistrée. Ne pas modifier une migration déjà distribuée: créer une nouvelle version. La vérification de migration doit se faire sur une copie de sauvegarde, jamais sur l'unique base de production.

## Compiler, tester, lancer

```powershell
dotnet restore MiniPharmacie.slnx
dotnet build MiniPharmacie.slnx
dotnet test MiniPharmacie.slnx
dotnet run --project MiniPharmacie.App
```

## Administration et sécurité

Au tout premier lancement, l'application demande de créer le premier compte responsable. Aucun mot de passe par défaut n'est fourni. Le mot de passe doit avoir au moins 12 caractères et est conservé avec un sel aléatoire via PBKDF2-HMAC-SHA256 (600 000 itérations). Le responsable peut créer ou désactiver des comptes vendeur et responsable. Les autorisations des services restent vérifiées à l'accès aux données, même si une commande d'interface est appelée directement.

Les vendeurs peuvent se connecter, consulter le catalogue, vendre, imprimer des tickets et consulter leurs propres rapports. Les fonctions utilisateurs, paramètres, rapports de stock/coûts et sauvegardes sont réservées au responsable. Le journal d'audit est append-only dans SQLite, avec des déclencheurs qui interdisent sa modification et sa suppression par l'application.

## Paramètres

Le responsable peut configurer le nom et les coordonnées de la pharmacie, le chemin du logo, la devise, l'imprimante, les décimales d'affichage, le délai de péremption, l'horaire de sauvegarde et le dossier de sauvegarde. Les taxes restent désactivées par défaut; leur activation est un choix explicite du responsable et aucune règle fiscale n'est présumée.

## Rapports

Les rapports de ventes sont filtrables par date et limités aux ventes du vendeur connecté. Les rapports responsables incluent les paiements, produits vendus, état du stock et ajustements. Ils peuvent être prévisualisés, imprimés ou exportés en CSV et PDF. La marge est indiquée comme marge brute indicative issue du coût historique des lots vendus; elle ne représente pas un bénéfice net. Les remboursements sont tracés à part des ventes.

## Sauvegardes et restauration

Le responsable peut créer une sauvegarde SQLite cohérente et vérifiée, la déposer dans un dossier distinct ou restaurer un fichier existant. Une sauvegarde vérifiée inclut un contrôle `PRAGMA integrity_check` et la présence des tables de l'application. La sauvegarde quotidienne s'exécute après l'horaire configuré pendant qu'une session responsable est ouverte; si l'application était fermée, elle s'exécute au prochain démarrage responsable. Le statut d'exécution est enregistré afin d'éviter plusieurs sauvegardes planifiées le même jour.

La restauration vérifie d'abord le fichier et demande confirmation. Elle exige que le compte responsable connecté soit présent dans la base source, crée une sauvegarde préalable vérifiée, restaure via l'API de sauvegarde SQLite, vérifie l'intégrité et restaure la base précédente en cas d'échec. Après une restauration réussie, redémarrer l'application. Conservez également une copie sur un support distinct du poste.

Les opérations proposées dépendent de la présence de NuGet pour restaurer `Microsoft.Data.Sqlite` et les packages xUnit. Si la restauration NuGet est bloquée par le réseau ou le proxy, les commandes de compilation et de test échoueront avant la compilation du code.

## Publication et installation Windows x64

Depuis PowerShell, à la racine du dépôt, restaurer les dépendances puis produire l’exécutable autonome et le paquet d’installation :

```powershell
dotnet restore .\MiniPharmacie.slnx
.\scripts\publish-windows.ps1
```

Le script publie l’application en Release, autonome (`win-x64`, `self-contained`) et en fichier unique. Il crée ensuite `artifacts\installer\win-x64\MiniPharmacie-Setup.exe` avec IExpress, intégré à Windows. L’installeur demande confirmation, affiche la licence MIT et installe l’application pour le compte Windows courant, sans droits administrateur. Pour publier uniquement l’application, le dossier `artifacts\publish\win-x64` contient le programme autonome.

L’installeur crée un raccourci sur le Bureau et dans le menu Démarrer. Il installe les fichiers sous `%LOCALAPPDATA%\Programs\MiniPharmacie`; la base reste sous `%LOCALAPPDATA%\MiniPharmacie\pharmacie.db`, séparée des binaires. Une mise à jour remplace les fichiers de programme uniquement. La désinstallation retire les binaires, raccourcis et entrée de désinstallation, mais conserve la base ainsi que les sauvegardes.

Au premier lancement, créer le compte responsable. Le compte Windows courant possède son propre dossier de données. Avant de saisir les données réelles, produire une sauvegarde vérifiée et conserver une copie sur un autre support.

Le paquet IExpress n’est pas signé numériquement. Windows peut afficher un avertissement SmartScreen lors du téléchargement ou du premier lancement. Une signature Authenticode nécessite un certificat de signature appartenant à l’éditeur.

## État connu et limites avant usage réel

- Présents dans l'interface: connexion, produits, ventes/caisse de base, utilisateurs, paramètres, rapports, sauvegarde/restauration.
- Les services de gestion des stocks/réceptions, sécurité, rapports et sauvegardes existent, mais l'interface n'intègre pas encore tout le parcours des catégories, fournisseurs, réception détaillée, inventaires, gestion complète des lots, retours et mouvements hors caisse.
- Aucun jeu de démonstration isolé, test automatisé de recherche sur 10 000 produits ni résultat de mesure inférieur à 2 secondes n'est livré/confirmé ici. Ne pas injecter de données d'essai dans la base par défaut.
- La suite xUnit est présente, mais l'exécution effective requiert que les packages soient restaurés. Un `dotnet test` qui n'affiche aucun cas exécuté n'est pas une preuve de réussite.
- Une validation métier/réglementaire reste nécessaire pour les retours/remboursements, taxes, clôtures, journaux, impressions, sauvegarde/restauration et droits d'accès avant une exploitation réelle.

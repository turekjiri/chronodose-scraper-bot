# Chronodose scraper bot pour Doctolib
- Bot pour récupérer les chronodoses sur le site https://www.doctolib.fr/
- Notifications via Slack
- Semble être plus précis que les sites tel que https://vitemadose.covidtracker.fr/, car basé directement sur la recherche en temps réel de Doctolib.

### TechStack
- C# .NET Core 3.1

[![forthebadge](https://forthebadge.com/images/badges/powered-by-black-magic.svg)](https://forthebadge.com)

### Windows 
#### Exécution
0. Assurez-vous d'avoir au moins le runtime de .NET Core 3.1 (https://dotnet.microsoft.com)
1. Télécharger la dernière Release depuis Github [https://github.com/turekjiri/chronodose-scraper-bot/releases] 
2. Exécuter `ChronodoseWatcher.App.exe` (pour la configuration des notifications, cf. Configuration)

#### Compilation depuis les sources 
0. Assurez-vous d'avoir le SDK de .NET Core 3.1 (https://dotnet.microsoft.com)
1. Télecharger le code source en .zip, ou cloner le repo.
2. Ouvrer un CMD/Powersheel/Terminal/... dans le dossier du projet (celui qui contient le fichier `ChronodoseWatcher.sln`)
3. Compiler le projet en exécutant la commande `dotnet build`
4. Allez dans le dossier `..ChronodoseWatcher.App\bin\Debug\netcoreapp3.1\`
5. Exécuter `ChronodoseWatcher.App.exe` (pour la configuration des notifications, cf. Configuration)

Ou utilisez tout simplement Visual Studio :)

### Linux
#### Compilation depuis les sources
0. Assurez-vous d'avoir le SDK de .NET Core 3.1 (https://docs.microsoft.com/en-us/dotnet/core/install/linux)
1. Télecharger le code source en .zip, ou cloner le repo.
2. Dans le répertoire contenant le fichier `ChronodoseWatcher.sln` lancer la commande `dotnet build` pour compiler la solution
3. Lancer l'application `./bin/Debug/netcoreapp3.1/ChronodoseWatcher.App` (pour la configuration des notifications, cf. Configuration)


### Configuration : config.json
Pour être notifié en temps réel sur Slack, il faut :
1. Trouver le fichier `config-example.json` et renommer le en `config.json` (celui qui est dans le même dossier que l'exécutable `ChronodoseWatcher.App.exe`)
2. Configurer un Webhook de Slack (https://api.slack.com/messaging/webhooks)
3. Remplace URL du webhook dans le fichier par votre propre URL.
```
{
    "slack": {
        "notify_slack" : true, 
        "webhook_url": "https://hooks.slack.com/services/AAA/BBBBB",
        "send_errors": false
    }
}
```
### Log : output-*date*.log
En plus des notifications et l'affichage dans la ligne de commande, tous les résultats dans enregistrés dans le fichier `output-*date_de_lancement_du_bot*.log`.

### TODO
- Limiter le rayon d'action à un certain kilométrage, paramétrable via fichier de config
- Ajouter d'autres providers de notifications

### Limitations
- Les résultats de recherche sont aussi à jour que la recherche directement sur Doctolib. Si Doctolib ne suit pas, bot non plus (et ça arrive plus souvent qu'on imagine)
- Pas de GUI, uniquement en ligne de commande
- Dépendant de Doctolib, peut donc arrêter de fonctionner à tout moment si Doctolib change leur fonctionnement
- **Aucune garantie de quoi que ce soit, utilisation à vos risques et périls**

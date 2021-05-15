# Chronodose scraper bot pour Doctolib
**TechStack : C# .NET Core 3.1**

Bot pour récupérer les chronodoses sur le site de Doctolib. 
Semble être plus rapide que les sites tel que https://vitemadose.covidtracker.fr/, car basé sur la recherche en temps réel de Doctolib.

### configuration : config.json
Pour être notifié en temps réel sur Slack, il faut :
1. Renommer le fichier `config-example.json` en `config.json`
2. Configurer un Webhook de Slack (https://api.slack.com/messaging/webhooks)
3. Remplace URL du webhook dans le fichier par votre propre URL.
```
{
    "slack": {
        "notify_slack" :  true, 
        "webhook_url": "https://hooks.slack.com/services/AAA/BBBBB",
        "send_errors": false
    }
}
```
### Log : output-*date*-*ville*.log
En plus des notifications et l'affichage dans la ligne de commande, tous les résultats dans enregistrés dans le fichier `output-*date*-*ville*.log`.

### Limitations
- Pas de GUI, uniquement en ligne de commande.
- Dépendant de Doctolib, peut donc arrêter de fonctionner à tout moment si Doctolib change leur fonctionnement
- **Aucune garantie de quoi que ce soit, utilisation à vos risques et périls**

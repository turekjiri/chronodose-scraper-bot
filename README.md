# Chronodose scraper bot pour Doctolib
**TechStack : C# .NET Core 3.1**

Bot pour r�cup�rer les chronodoses sur le site de Doctolib. 
Semble �tre plus rapide que les sites tel que https://vitemadose.covidtracker.fr/, car bas� sur la recherche en temps r�el de Doctolib.

### configuration : config.json
Pour �tre notifi� en temps r�el sur Slack, il faut :
1 - Renommer le fichier `config-example.json` en `config.json`
2 - Configurer un Webhook de Slack (https://api.slack.com/messaging/webhooks)
3 - Remplace URL du webhook dans le fichier par votre propre URL.

    {
    	  "slack": {
    		"notify_slack" :  true, 
    		"webhook_url": "https://hooks.slack.com/services/AAA/BBBBB",
    		"send_errors": false
    	}
    }

### Log : output-*date*-*ville*.log
En plus des notifications et l'affichage dans la ligne de commande, tous les r�sultats dans enregistr�s dans le fichier `output-*date*-*ville*.log`.

### Limitations
- Pas de GUI, uniquement en ligne de commande.
- D�pendant de Doctolib, peut donc arr�ter de fonctionner � tout moment si Doctolib change leur fonctionnement
- **Aucune garantie de quoi que ce soit, utilisation � vos risques et p�rils**
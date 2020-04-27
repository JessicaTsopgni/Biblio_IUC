##Pour VS:
- Démarrer en tant admin l'IDE.
	- Menu Tools -> Command Line -> Developer command line
	- > cd BiblioIUC
	- > dotnet watch run
	
	- Scaffolding en cas de modification de la DB
		- Menu Tools -> Nuget package manager -> Package manager console 
		> Scaffold-DbContext -connection name=MySql -provider Pomelo.EntityFrameworkCore.MySql -context BiblioEntities -o Entities -f
	**NB: Le projet doit être compilable *(pas de bug)***
	
	
##VS Code: 
	- Démarrer en tant admin l'IDE.
	- Fermer le dossier en cours si existe.
	- Glisser-déposer le repertoire ./BiblioIUC/BiblioIUC dans l'IDE
	- Menu Terminal -> New terminal
	- > dotnet watch run
	- Scaffolding en cas de modification de la DB : *Pas encore de solution*

##Notes importantes pour votre rapport:
	- La base de données est dans le repertoire ./Db/biblio_iuc_sql.
	- La chaine de connexion se trouve dans le fichier ./appsettings.Development.json.
	- Il faudra privilégier le terme document, plus générique *(livre, rapport/memoire, cours,..., qui sont des catégories de document)* , à celui de livre.
	- Un document peut faire partir de plusieurs catégories mais pour le cas d'étude on retiendra seulement la catégorie principale.
	- Les catégories en noir sont sont désactivées `Status = 0` et accéssibles uniquement par un le bibliothècaire `admin, Role = 0`. Les autres couleurs sont purement design. 
	- Néanmoins il faudra créer *(manuellement dans le bd)* un compte étudiant de `Role = 1` et/ou enseignant de `Role = 2`, pour vérifier ce fait.
	- Vérifier aussi que les comptes etudiants et enseignants sont véritablement restreints. 
	- Le nombre de document affiché pour les catégories est fictif pour l'instant.
	
##Références utiles
	- [Métadonnées du livre et informations sur le livre](https://support.google.com/books/partner/answer/3237055?hl=fr)

## Constellations
Le projet constellations s'inscrit dans la suite du projet MagicaVoxel. Le but est de faire une mise en pratique du ScriptedImporter sur un autre format de fichier.
Le projet est accessible sur [cette page notion](https://www.notion.so/samtechart/Projet-Constellations-28a5d96b946d80659581d73eaee2baef).

### Commande
Un studio de jeu vidéo réalise un jeu utilisant des constellations d'étoiles. Afin d'avoir une bonne base de constellations, ils prévoient d'utiliser directement une grosse database de constellations qui existe déjà au format .SVG. Le soucis est qu'Unity ne permet d'importer nativement un format SVG et les plugins existant propose de les convertir sous forme de texture mais pas d'en tirer des coordonnées.

Le Studio a donc besoin d'un outil permettant d'importer des fichier .SVG dans Unity, d'en lire les coordonnées des tracés pour pouvoir les utiliser dans le projet.

### Étapes
- Faire quelque recherches sur l'extension de fichier SVG.

- Créer un ScriptedImporter & un ScriptableObject pour contenir les nouvelles données importées.

- Ajouter la library System.Xml et apprendre à l'utiliser.

- Comprendre le formatage des paths du SVG et en importer les coordonnées dans Unity.
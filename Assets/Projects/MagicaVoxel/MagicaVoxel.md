## MagicaVoxel
Le projet MagicaVoxel fait une introduction aux ScritedImporter. L'objectif est d'apprendre à lire un fichier simple pour en faire des données exploitables dans Unity.
Le projet est accessible sur [cette page notion](https://www.notion.so/samtechart/Projet-MagicaVoxel-2765d96b946d8064b11bc1616a245fbe).

### Commande
Un studio de jeu vidéo crée son jeu avec [MagicaVoxel](https://ephtracy.github.io/), tout les assets sont créés sur base de voxels. Les développeurs aimeraient faire en sorte que certains assets puissent être explosés sous forme de petits voxels indépendants.

Le studio a donc besoin d’un outil permettant de générer des explosions de voxels depuis leurs créations réalisées sur MagicaVoxel.

### Étapes
- Étant donné que les objets exportés en .obj ne nous donnent pas vraiment les voxels d’origine, nous devons trouver un autre format d’export qui nous permet d’avoir les voxels.

- Une fois le format trouvé, il est peu probable que ce format soit supporté de base dans Unity *(Sinon il n’y aurait probablement pas de commande)* nous devons donc faire notre propre importer pour ce format.

- Étant donné le nombre de voxels que cela peut représenter, nous utiliserons certainement un Visual Effect Graph du côté d’Unity pour faire l’effet d’explosion. Il nous faudra donc convertir nos données dans un buffer. Un pour les positions et un autre pour les couleurs.
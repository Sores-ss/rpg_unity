# EldenPixel — RPG 2D top-down sur Unity

> Projet réalisé dans le cadre d'un **User Group Epitech** dont l'objectif était d'apprendre à créer un jeu vidéo avec **Unity**. Le résultat est un petit RPG 2D en vue du dessus, inspiré de l'ambiance et des mécaniques de *Elden Ring* (combat au corps-à-corps, blocage, boss à vaincre), en pixel art.

---

## Sommaire

- [Contexte](#contexte)
- [Le jeu](#le-jeu)
- [Contrôles](#contrôles)
- [Fonctionnalités](#fonctionnalités)
- [Stack technique](#stack-technique)
- [Installation](#installation)
- [Jouer sans Unity](#jouer-sans-unity)
- [Lancer le jeu](#lancer-le-jeu)
- [Compiler en ligne de commande](#compiler-en-ligne-de-commande)
- [Structure du projet](#structure-du-projet)
- [Architecture des scripts](#architecture-des-scripts)
- [Assets utilisés](#assets-utilisés)
- [Problèmes connus / pistes d'amélioration](#problèmes-connus--pistes-damélioration)
- [Auteur](#auteur)

---

## Contexte

Ce projet a été développé lors d'un **User Group** organisé au sein d'**Epitech**. Les User Groups sont des ateliers animés par des étudiants pour des étudiants, autour d'une technologie ou d'un sujet précis. Celui-ci portait sur la **création d'un jeu sous Unity** : prise en main de l'éditeur, scripting C#, système d'input, tilemaps, animations, UI, audio et export d'une build.

Le jeu n'a pas vocation à être un produit fini : c'est un terrain d'apprentissage qui met en pratique les notions vues pendant l'atelier.

## Le jeu

Vous incarnez un chevalier qui explore un monde médiéval à partir d'un hub central relié à quatre zones (nord, sud, est, ouest). Vous affrontez des ennemis, ramassez du butin et de l'or, montez de niveau auprès d'un PNJ, ouvrez des coffres, et descendez dans un donjon lui aussi généré procéduralement pour vaincre le **boss**. La partie est gagnée quand le boss tombe ; elle est perdue quand vos PV atteignent zéro.

## Contrôles

Le jeu utilise le **nouveau Input System** d'Unity. Les touches sont mappées pour fonctionner à la fois en **AZERTY** et en **QWERTY**.

| Action | Touche(s) |
|---|---|
| Se déplacer | `Z` `Q` `S` `D` / `W` `A` `S` `D` |
| Attaquer / interagir avec un objet équipé | `Espace` ou `J` |
| Interagir (coffre, PNJ, objets) | `F` |
| Se faire soigner (PNJ soigneur) | `H` |
| Ouvrir / fermer les statistiques et monter de niveau | `Tab` |
| Sélectionner un slot de la barre rapide | `1` à `9` |
| Naviguer dans la barre rapide | `←` / `→` |
| Utiliser l'objet sélectionné | `E` |
| Fermer un coffre | `Échap` |
| Rejouer après un Game Over | `R` |

## Fonctionnalités

- **Joueur** : déplacement 4 directions, animations pilotées par `PlayableGraph` (idle / course / attaque / blocage), attaque au corps-à-corps avec zone d'impact, recul (knockback) et temps de recharge.
- **Santé & progression** : points de vie, blocage qui réduit les dégâts de moitié, système de niveaux acheté en **or** (PV, attaque et vitesse augmentent à chaque niveau, coût exponentiel), UI de stats avec aperçu du prochain niveau.
- **Ennemis** : IA en trois états (errance / poursuite / attaque), barre de vie procédurale, résistance au recul, spawner, table de butin configurable (`EnemyLootDrop`) avec probabilité et quantité par objet, boss déclenchant la victoire.
- **Inventaire** : 24 slots, ramassage d'objets au sol, barre rapide à 9 slots, objets utilisables.
- **Coffres** : grille d'inventaire dédiée avec transfert d'objets vers/depuis le joueur.
- **Dialogues** : PNJ avec système de dialogues simples (`SimpleDialogueUI` + `DialogueData`), PNJ soigneur.
- **Génération procédurale** :
  - `WorldMapGenerator` : hub central + 4 zones avec des `TileSet` différents par zone.
  - `DungeonGenerator` : salles aléatoires reliées par des couloirs, murs avec collisions automatiques.
  - `StairsTrigger` : escaliers pour passer d'un niveau à l'autre.
- **Monde** : objets destructibles, gestion des niveaux de hauteur (`HeightLevelGroup`), caméra qui suit le joueur.
- **UI & audio** : menu principal, écran de Game Over, écran de victoire, musique de fond et sons d'action.
- **Rendu** : Universal Render Pipeline (URP) 2D.

## Stack technique

| Élément | Version |
|---|---|
| Unity | **6000.3.9f1** (Unity 6) |
| Langage | C# |
| Rendu | Universal Render Pipeline 17.3 |
| Input | Input System 1.18 |
| 2D | 2D Animation, 2D Tilemap (+ Extras), Sprite Shape, Aseprite / PSD Importer |
| UI | uGUI + TextMesh Pro |

La liste complète des packages est dans [`RPG/Packages/manifest.json`](RPG/Packages/manifest.json).

## Jouer sans Unity

Une build est disponible dans les [**Releases**](https://github.com/Sores-ss/rpg_unity/releases) du dépôt :

```bash
unzip EldenPixel-Linux.zip
cd Linux
chmod +x EldenPixel.x86_64
./EldenPixel.x86_64
```

Sous Windows, dézipper `EldenPixel-Windows.zip` et lancer `EldenPixel.exe`.

## Installation

### Prérequis

- [Unity Hub](https://unity.com/download)
- Unity **6000.3.9f1** (installer le module *Linux Build Support* / *Windows Build Support* selon la plateforme cible si vous voulez exporter une build)
- Git

### Récupérer le projet

```bash
git clone git@github.com:Sores-ss/rpg_unity.git
cd rpg_unity
```

### Ouvrir dans Unity

1. Ouvrir **Unity Hub** → **Add** → sélectionner le dossier `RPG/` (et non la racine du dépôt).
2. Laisser Unity réimporter les assets et régénérer le dossier `Library/` (quelques minutes au premier lancement).
3. Ouvrir la scène `Assets/Scenes/MainMenu.unity`.

> Les dossiers `Library/`, `Temp/`, `Logs/`, `UserSettings/` ainsi que les fichiers `.csproj` / `.sln` sont générés par Unity et volontairement exclus du dépôt (voir [`.gitignore`](.gitignore)).

## Lancer le jeu

### Depuis l'éditeur

Ouvrir `Assets/Scenes/MainMenu.unity` et appuyer sur **Play**. Le menu charge ensuite la scène `Game.unity`.

### Exporter une build depuis l'éditeur

Le menu **Build** (ajouté par `Assets/Editor/BuildScript.cs`) propose **Linux x64**, **Windows x64** et **All platforms**. Les builds sont écrites dans `Builds/<Plateforme>/` à la racine du dépôt, hors du projet Unity. Vous pouvez aussi passer par **File → Build Profiles** comme d'habitude.

## Compiler en ligne de commande

Le script [`build.sh`](build.sh) pilote l'éditeur Unity en mode batch (sans interface) et produit un zip prêt à distribuer :

```bash
./build.sh            # build Linux (par défaut)
./build.sh windows    # nécessite le module "Windows Build Support (Mono)" dans Unity Hub
./build.sh all
```

Sortie : `Builds/Linux/`, `Builds/Windows/` et les archives `Builds/EldenPixel-<Plateforme>.zip`. Le log complet est dans `Builds/build.log`.

Le script cherche l'éditeur correspondant à `ProjectSettings/ProjectVersion.txt` dans `~/Unity/Hub/Editor/` ; sinon indiquer le chemin avec `UNITY_PATH=/chemin/vers/Unity ./build.sh`. Unity doit être installé et **le compte Unity connecté dans Unity Hub** (la licence Personal est vérifiée au lancement), et le projet ne doit pas être ouvert dans l'éditeur.

## Structure du projet

```
rpg_unity/
├── .gitignore
├── README.md
├── build.sh                      # Build en ligne de commande
├── Builds/                       # Sorties de build (ignoré par git)
└── RPG/                          # Projet Unity (à ouvrir dans Unity Hub)
    ├── Assets/
    │   ├── Audios/               # Musiques et effets sonores
    │   ├── Editor/               # Scripts éditeur (dont BuildScript.cs)
    │   ├── PNJ/                  # Assets des personnages non-joueurs
    │   ├── Player/               # Sprites et animations du joueur
    │   ├── Prefabs/              # Prefabs (ennemis, objets, UI…)
    │   ├── Resources/
    │   ├── Scenes/               # MainMenu.unity, Game.unity
    │   ├── Scripts/              # Tout le code gameplay (C#)
    │   ├── Settings/             # Profils URP
    │   ├── Sprites/              # Packs de pixel art
    │   ├── TextMesh Pro/
    │   ├── Tilemap/              # Tiles et TileSets
    │   └── InputSystem_Actions.inputactions
    ├── Packages/                 # manifest.json + lock
    └── ProjectSettings/          # Réglages du projet Unity
```

## Architecture des scripts

Tous les scripts sont dans [`RPG/Assets/Scripts/`](RPG/Assets/Scripts/).

| Domaine | Scripts |
|---|---|
| Joueur | `Move_Player`, `PlayerAttack`, `PlayerHealth`, `PlayerHealthUI`, `PlayerAnimations`, `PlayerStats`, `PlayerStatsUI`, `PlayerLevel` |
| Ennemis | `EnemyCombat`, `EnemyData`, `EnemyAnimation2D`, `EnemySpawner`, `EnemyLootDrop`, `LootEntry` |
| Inventaire & objets | `InventoryManager`, `InventoryUI`, `HotbarSlot`, `PickupItem`, `ItemAnimationType`, `ChestSystem`, `ChestSlot` |
| PNJ & dialogues | `NpcHealer`, `SimpleDialogueUI`, `DialogueData` |
| Monde & génération | `WorldMapGenerator`, `DungeonGenerator`, `MapSceneBuilder`, `TileSet`, `StairsTrigger`, `HeightLevelGroup`, `DestructibleObject` |
| Caméra | `Follow_Player`, `ManagePlayerDistance` |
| UI & audio | `MainMenuUI`, `GameOverUI`, `WinUI`, `BackgroundMusic` |

Quelques choix notables :

- `InventoryManager` et `PlayerStats` sont des **singletons** (`Instance`) accessibles depuis n'importe quel script.
- La communication entre systèmes passe majoritairement par des **événements C#** (`PlayerStats.LevelChanged`, `EnemyCombat.BossKilled`, …) pour éviter les dépendances directes.
- Les animations du joueur utilisent l'API **Playables** plutôt qu'un Animator Controller, ce qui permet de changer de clip directement depuis le code.
- Les données de configuration (stats des ennemis, tables de butin, dialogues, tilesets) sont exposées dans l'inspecteur via `[SerializeField]` pour être réglées sans toucher au code.

## Assets utilisés

Les graphismes proviennent de packs de pixel art gratuits (dossier `Assets/Sprites/`), notamment :

- *Pixel Art Top Down – Basic*
- *32x32 Pixel Knight*
- *Pixel Art Furniture Pack – FREE*

Chaque pack conserve sa licence d'origine dans son dossier. Ils sont utilisés uniquement à des fins pédagogiques.

## Problèmes connus / pistes d'amélioration

- `PlayerHealth.Die()` notifie le Game Over via `SendMessage` alors que le reste du projet utilise des événements C#.
- Pas de sauvegarde de partie.
- Pas de support manette côté gameplay (les touches sont lues directement via `Keyboard.current`).
- Le nom de produit dans `ProjectSettings` est encore celui par défaut (`My project`).

## Auteur

**Eros** — [Sores-ss](https://github.com/Sores-ss)

Projet réalisé dans le cadre d'un User Group Epitech.

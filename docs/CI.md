# Intégration continue (GitHub Actions)

Le workflow [`.github/workflows/build.yml`](../.github/workflows/build.yml) compile le jeu pour
**Linux** et **Windows** avec [GameCI](https://game.ci) à chaque push sur `main`, et publie une
**GitHub Release** avec les builds zippées quand un tag `v*` est poussé.

## 1. Configuration initiale : secrets de licence Unity

GameCI a besoin d'une licence Unity pour lancer l'éditeur dans le cloud. Une licence **Personal**
(gratuite) suffit.

1. Générer le fichier d'activation sur ta machine (adapter la version si besoin) :

   ```bash
   ~/Unity/Hub/Editor/6000.3.9f1/Editor/Unity -batchmode -nographics -quit \
       -createManualActivationFile -logFile -
   ```

   Cela crée `Unity_v6000.3.9f1.alf` dans le dossier courant.
2. Aller sur <https://license.unity3d.com/manual>, envoyer le fichier `.alf`, choisir **Personal**,
   et télécharger le fichier `Unity_v6000.x.ulf` obtenu.
3. Dans le dépôt GitHub : **Settings → Secrets and variables → Actions → New repository secret** :

   | Nom du secret    | Valeur                                   |
   |------------------|------------------------------------------|
   | `UNITY_LICENSE`  | le contenu complet du fichier `.ulf`     |
   | `UNITY_EMAIL`    | l'email du compte Unity                  |
   | `UNITY_PASSWORD` | le mot de passe du compte Unity          |

Ne jamais commiter les fichiers `.alf` / `.ulf` (ils sont dans le `.gitignore`).

## 2. Publier une release

```bash
git tag v1.0.0
git push origin v1.0.0
```

Le workflow compile les deux plateformes puis crée une release `v1.0.0` avec
`EldenPixel-StandaloneLinux64.zip` et `EldenPixel-StandaloneWindows64.zip` en pièces jointes.

## 3. Lancement manuel

**Actions → Build → Run workflow** compile `main` sans publier ; les zips sont disponibles
comme artefacts du workflow pendant 14 jours.

## Remarques

- Le premier run prend ~15–25 minutes (le dossier `Library/` est reconstruit de zéro) ; les
  suivants réutilisent le cache.
- Les images Docker GameCI suivent les versions Unity avec un léger délai : si le run échoue avec
  une image introuvable pour `6000.3.9f1`, vérifier la disponibilité sur
  <https://hub.docker.com/r/unityci/editor/tags>.
- Deux builds tournent en parallèle ; le quota gratuit de GitHub Actions est largement suffisant
  pour un projet de cette taille.

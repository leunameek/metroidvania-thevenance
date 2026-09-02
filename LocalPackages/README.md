# Local package tarballs

This directory stores large Unity Package Manager tarballs required by the project.
Binary tarballs are tracked with Git LFS so package references can stay portable across
developer machines.

## MediaPipe Unity Plugin 0.16.3

- File: `com.github.homuler.mediapipe-0.16.3.tgz`
- Source: https://github.com/homuler/MediaPipeUnityPlugin/releases/tag/v0.16.3
- SHA-256: `cc3e77a219e0b99618ae3be64c31a566197deedc69c1e136acf52d65d7cf2e79`

`Packages/manifest.json` references this file as `../LocalPackages/...`, relative to the
`Packages` directory. Keeping this folder at the project root prevents Unity from mistaking it
for an embedded package, because direct child folders under `Packages/` must contain their own
package manifest.

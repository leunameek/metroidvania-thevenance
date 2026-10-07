# Natural input: hands and voice (2026-10-06)

Hands (MediaPipe hand landmarker) and voice (Windows `KeywordRecognizer`) drive the same game
actions as the keys in Plaza Núñez, Mundo Inferior and Mundo Superior. Keys always keep working.

## Pieces

| Piece | Assembly | Role |
| --- | --- | --- |
| `HandGestureModel` | Model | Open/closed per hand, open-hand deltas, palm position. |
| `HandGestureRecognizerModel` | Model | Gestures from both hands: `Strike` (open → fist), `Swipe` (fast sideways open hand, mirrored direction), `PalmHold` (still open palm for the dwell time), `Grab` (fist held 0.7 s) as events with a 0.4 s lifetime, and `GuardHeld` (two raised open palms, 0.25 s) as a state. Holds re-arm only after the hand opens/closes or leaves. `ClearPending` drops events at every new turn/window/screen. EditMode tests: `HandGestureRecognizerModelTests`. |
| `HandGestureTracker` | Runtime | Converts MediaPipe results (background thread, lock + clone) into both models every camera result. |
| `HandTrackingSession` | Runtime | Loads the additive "Hand Landmark Detection" scene on demand (C), hides its camera/UI. `Wanted` survives scene changes, so the next scene's session restarts the camera by itself. The plaza's bootstrap prefab (bundled model, StreamingAssets loader) is shared with the world sessions. `PlazaHandSession` is the plaza subclass (mouse fallback text). |
| `VoiceCommandRecognizer` | Runtime | Keyword list → `VoiceCommand { Dodge, Attack, Guard, Interact, Confirm, Back }`. `Simulate(phrase)` drives the same path for tests. |
| `WorldNaturalInput` + `NaturalInputHUD` | Runtime | Created by `MundoInferiorBlockout` / `MundoSuperiorDirector`. One query per action (`ConsumeInteract/Confirm/Back/Attack/Guard/Dodge`, `ConsumeRotate`), a per-frame `NaturalContext` set by whoever owns the moment (director, find, guardian), a HUD plate (camera/voice state, gesture guide, tracked-hand dots), a hold bar under the interaction ribbon and a feedback ribbon. Voice and camera toggles are added to the world pause (`MIHud.AddPauseButton`). Settings are read from the plaza's stored UI settings (`NaturalInputPrefs`). |

## Mapping

- Plaza: `HandTrackingUIController` dwell (open palm) calls `TechnicalDemoController.Interact` for stations, portals and the training circle; a held fist ends a completed lesson; `PlazaCombatController.UpdateGestures` maps Strike / Swipe / GuardHeld / Grab to attack / dodge / guard / leave; `VoiceUIController` adds interact, back and confirm words outside the duel. Turning the camera on switches to hands mode (and off to mouse).
- Mundo Inferior: interaction and finds as above; Strike (only near the shield sentinel or the guardian fight) and «impulso» (anywhere) call `PlayerController.RequestDash`, the same path as Q.
- Mundo Superior: interaction and finds as above; `MSGuardian` accepts Strike/«atacar» in the player turn and GuardHeld/«bloquea», Swipe/«esquiva» in the reaction window, which now also uses the plaza's reaction-time setting.

## Validation

EditMode 48/48. PlayMode walkthroughs in a scratch copy (fake camera results fed into the real
tracker models, voice via `Simulate`): plaza station → lesson → full training won with gestures
only and no mistakes; Mundo Inferior seed opened by voice and taken by a held fist, bracelets
opened by a palm hold and returned with «salir», voice dash; Mundo Superior duel started by
voice, fist and voice attacks, guard pose block and swipe dodge without damage.
Not verified here: a live webcam and microphone (left/right mirroring of the swipe, open/closed
thresholds and Windows speech language need a real session).

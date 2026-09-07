# Project Slayers - Codex Instructions

## 1. Project

Project Slayers is an active Unity 2D action game project.

This repository is the current game-development repository.

Old prototypes, old prototype code, prototype-analysis documents, archived implementations,
and deprecated architecture are reference material only.

Do not directly port old prototype architecture into the current project unless the task
explicitly asks for a specific piece of legacy behavior to be reused.

The goal is to build the current project with maintainable, extensible systems suitable
for increasing numbers of characters, skills, enemies, equipment, stages, and combat mechanics.


---

## 2. General Working Rules

Before modifying code:

- Read the existing implementation related to the task.
- Identify the current ownership and data flow before changing architecture.
- Preserve currently working behavior unless the task explicitly changes it.
- Do not invent missing specifications.
- Do not silently make game-design decisions.
- Do not invent balance values, skill behavior, stats, character settings, lore, story details,
  visual canon, or combat rules.
- If required information is missing or ambiguous, report it instead of choosing a design yourself.
- Prefer small, reviewable changes over large unsolicited rewrites.
- Do not modify unrelated files.
- Do not refactor unrelated systems only because they could theoretically be improved.
- Keep the implementation within the requested scope.

Before finishing a task, report:

- files changed
- files created
- files deleted, if any
- behavior changed
- architectural changes, if any
- tests or verification actually performed
- anything that could not be verified
- unresolved issues
- specifications that are still missing


---

## 3. Project Documentation and Canon

When a task depends on any of the following:

- game design
- combat rules
- stats
- damage calculations
- skills
- characters
- equipment
- economy
- enemies
- bosses
- stages
- story
- worldbuilding
- factions
- visual canon
- VFX rules

read the relevant project documentation under `Docs/` before implementing the task.

Do not infer missing design specifications from code alone when authoritative project
documentation should exist.

If `Docs/` does not exist, or the required source document is unavailable:

- do not invent the missing specification
- do not silently choose a temporary design
- report that the required source document is unavailable
- continue only with portions of the task that do not depend on the missing specification


---

## 4. Source Priority

When project documents contain overlapping or conflicting information, use this priority:

1. The user's latest explicit decision.
2. The latest `[ACTIVE UPDATE]`.
3. `[CONFIRMED]` information.
4. Preserved original material that has not been superseded.
5. Primary Canon visual assets when the issue is visual appearance.
6. Unresolved or provisional material only as reference.

The following markers must not be silently finalized:

- `[CHECK]`
- `[TEMP]`
- prototype
- provisional
- unresolved

Do not remove those states unless the user explicitly resolves them.

Do not restore or prefer `[DEPRECATED]` material over a newer active definition.

If two sources still conflict after applying the priority rules:

- report the conflict
- identify the relevant files or sections
- do not choose a new interpretation without instruction


---

## 5. Architecture Principles

The current project must not reproduce the oversized, tightly coupled manager architecture
used in older prototypes.

Prefer clear ownership, explicit data flow, reusable systems, and components with focused responsibilities.

Avoid:

- giant central manager classes
- a single manager directly controlling unrelated systems
- numeric IDs being used as gameplay meaning
- large switch statements based on numeric skill IDs
- array positions being used as undocumented stat identities
- Transform.GetChild(index) dependencies for important gameplay or UI behavior
- Animator state names being treated as authoritative gameplay state
- one boolean field per skill
- one unique damage MonoBehaviour per skill when a reusable system is appropriate
- duplicated object-pool implementations
- direct runtime mutation of ScriptableObject assets as save data
- direct runtime mutation of ScriptableObject assets as character-instance state
- hard-coded references that make additional characters or skills require large rewrites

Prefer:

- explicit types
- enums where appropriate
- clearly named data structures
- small components with clear ownership
- reusable systems
- inspector-visible references where appropriate
- data/runtime-state separation
- composition over large inheritance hierarchies when practical
- systems that remain extensible as content increases


---

## 6. Action System

Animator is an output layer.

Animator must not decide authoritative player or gameplay state.

The gameplay controller, such as `ActionController`, owns action state and determines
what action is currently active.

Player actions use the following phases where applicable:

- `Startup`
- `Active`
- `EarlyRecovery`
- `LateRecovery`
- `Finished`

The exact timing of these phases is action-specific and must come from game data or
explicit implementation requirements.

Do not invent timings.


---

## 7. Movement Authority

Actions may control movement using the following authority states:

- `Locked`
- `Reduced`
- `Free`

Meaning:

- `Locked`: normal movement input must not control movement.
- `Reduced`: movement is permitted under action-specific restrictions.
- `Free`: normal movement is available.

Do not automatically force horizontal velocity to zero for every action.

If an action requires movement locking, reduced movement, forced movement, impulse,
or root-motion-like behavior, implement it explicitly.


---

## 8. Cancel and Interrupt Rules

Normal action chaining must check whether the current cancel window permits the requested action.

Forced cancellation uses:

- `CancelPower`
- `InterruptArmor`

A forced interruption should compare the incoming action/control effect against the
current action's interrupt protection.

Do not bypass this structure with unrelated special-case booleans unless the task explicitly
requires a justified exception.

Higher-priority control states include, where applicable:

- `HitStun`
- `KnockDown`
- `Grabbed`
- `Dead`
- `Cutscene`

These states may override normal action flow according to the project's control rules.


---

## 9. Action Completion and Cleanup

Every action must have explicit termination paths.

Normal termination:

- `CompleteAction`

Forced or interrupted termination:

- `CancelAction`

Both paths must reach shared cleanup behavior.

Cleanup must release or restore any temporary state created by the action.

Depending on the action, cleanup may include:

- hitboxes
- colliders
- temporary movement locks
- temporary movement modifiers
- spawned effects
- trails
- particles
- coroutines
- timers
- event subscriptions
- targeting data
- hit target lists
- temporary stat modifiers
- animation-related transient flags
- pooled objects
- temporary input restrictions

An action must not depend on reaching its normal final animation frame in order to clean itself up.


---

## 10. Object Pooling

Reusable combat objects should use explicit pool lifecycle behavior.

Use explicit concepts equivalent to:

- Spawn
- Release
- OnSpawned
- OnDespawned

When a pooled object is reused, reset runtime state as applicable.

This includes:

- Rigidbody velocity
- Rigidbody angular velocity
- timers
- elapsed time
- lifetime state
- hit target collections
- hit counters
- colliders
- trails
- particle systems
- target references
- ownership references
- temporary modifiers
- event subscriptions
- active flags that are not automatically restored correctly

Do not assume that disabling and re-enabling a GameObject automatically restores a valid reusable state.


---

## 11. Stats and Runtime Data

Static definition data and runtime instance data must remain conceptually separate.

Do not use ScriptableObject definition assets as mutable runtime save-state containers.

Prefer separate structures for concepts such as:

- character definition
- base stats
- runtime stats
- stat modifiers
- current resources
- equipment instance
- skill definition
- skill runtime state

Current resources such as HP, MP, gauges, cooldowns, charges, and temporary combat state
must not be treated as immutable definition data.

Do not invent new stats or remove existing stats without explicit design documentation.


---

## 12. Damage and Combat Calculations

When implementing damage or combat formulas:

- read the relevant active game-system documentation first
- use the documented formula exactly
- preserve documented storage units
- do not substitute a more conventional formula because it appears preferable
- do not invent caps, rounding rules, minimum damage, maximum damage, immunity behavior,
  fixed damage behavior, shield order, or damage-processing order when those are not specified

If a calculation rule is marked unresolved, keep it unresolved and report the missing decision.


---

## 13. Skills

Skills must be implemented from the authoritative skill specification.

Do not invent:

- damage coefficients
- cooldowns
- resource costs
- hit counts
- hit timing
- status durations
- movement distance
- cancel timing
- effect size
- VFX behavior
- upgrade effects
- prerequisite rules

unless the task explicitly asks Codex to propose candidate values.

When candidate values are requested, clearly mark them as proposals rather than canon.

Avoid architecture where each newly added skill requires:

- adding a global boolean
- adding a numeric switch branch
- creating duplicated damage-processing logic
- modifying unrelated character code

Prefer reusable actions and data-driven configuration where appropriate.


---

## 14. Unity Serialized Assets

Do not modify serialized Unity assets unless the task requires it.

This includes:

- Scenes
- Prefabs
- Animator Controllers
- Animation Clips
- ScriptableObject data assets
- Input Action assets
- Timeline assets
- ProjectSettings
- Package configuration
- serialized material or VFX assets

When source-code-only work is sufficient, leave serialized assets untouched.

If serialized assets must be modified:

- keep the modification scoped
- preserve unrelated serialized data
- do not reorder or regenerate unrelated content
- report exactly which serialized assets changed


---

## 15. Animator

Animator is not the gameplay-state authority.

Do not use Animator state inspection as the primary mechanism for deciding:

- whether the player may move
- whether a skill is active
- whether another action may cancel
- whether hitboxes should exist
- whether the character is invulnerable
- whether the action has completed

Gameplay state should drive Animator output rather than Animator output driving gameplay state.

Animation Events may be used for frame-specific presentation or explicitly designed gameplay events,
but action safety must not rely exclusively on a normal animation ending.


---

## 16. Unity Verification

Never claim that Unity compilation succeeded unless Unity compilation was actually executed.

Never claim that Play Mode testing succeeded unless Play Mode was actually executed.

Never claim that a Scene, Prefab, Animator Controller, animation, VFX, physics interaction,
or Inspector setup is correct unless it was actually verified.

If Unity is unavailable in the cloud environment, clearly state the verification limitation.

Acceptable wording includes:

- source inspection completed
- static code review completed
- available automated tests passed
- Unity compilation was not available
- Play Mode verification is still required
- Inspector configuration must be checked locally
- serialized references could not be verified in the cloud environment

Do not convert unavailable verification into assumed success.


---

## 17. Warnings and Errors

Do not hide errors merely to make the console appear clean.

Do not suppress compiler warnings without understanding their cause.

Do not remove validation code simply because it reports a problem.

When fixing an error:

1. identify the underlying cause
2. fix the cause at the correct ownership layer
3. avoid unrelated architectural rewrites unless necessary
4. report any behavior change caused by the fix


---

## 18. File Scope

Before modifying a file, determine whether the requested task actually requires that file.

Avoid touching unrelated:

- scripts
- prefabs
- scenes
- materials
- animations
- project settings
- package files
- generated files

Do not perform formatting-only changes across the repository unless explicitly requested.

Do not rename files, classes, namespaces, serialized fields, folders, or public APIs without
considering Unity serialization and existing references.


---

## 19. Generated Files

Do not manually edit files or directories that Unity or development tools regenerate unless required.

Examples include generated or local-only data such as:

- `Library/`
- `Temp/`
- `Logs/`
- `obj/`
- generated IDE project files

Respect the repository's `.gitignore`.


---

## 20. Git Rules

Treat `main` as the stable branch.

For substantial feature work:

- prefer a separate branch or worktree
- keep the work isolated until reviewed
- keep commits scoped to the requested task
- use descriptive commit messages
- do not mix unrelated changes into one commit

Do not:

- force-push
- rewrite published history
- perform destructive reset operations
- delete branches containing unmerged work
- discard user changes
- overwrite unrelated local modifications

unless explicitly instructed.

Before destructive Git operations, stop and request confirmation.

Do not merge substantial work into `main` merely because implementation is finished.
The user should be able to review the work first.


---

## 21. Existing User Changes

User-authored changes have priority over Codex cleanup preferences.

If files already contain uncommitted changes:

- inspect them before editing
- do not overwrite them
- do not revert them
- do not assume they are accidental
- preserve unrelated user modifications

If a task conflicts with existing user changes, report the conflict.


---

## 22. Refactoring

Refactor only when:

- required to complete the task safely
- explicitly requested
- necessary to prevent a clearly identified architectural problem

Do not rewrite a functioning subsystem solely because a different architecture appears cleaner.

For major refactors:

1. explain the problem
2. identify affected systems
3. preserve behavior
4. keep the change reviewable
5. identify migration risks
6. verify as much as the environment allows


---

## 23. Extensibility

When multiple valid implementations exist, prefer the one that reasonably supports future expansion.

The project is expected to grow in:

- playable characters
- skills
- bosses
- enemy types
- status effects
- equipment
- sets
- stages
- combat mechanics
- UI panels
- save data

However, do not create speculative abstraction layers for hypothetical features that have not been requested.

Prefer practical extensibility over premature generalization.


---

## 24. Game Design vs Implementation

Separate implementation problems from design decisions.

Codex may independently solve implementation details such as:

- class organization
- reusable interfaces
- internal helper methods
- code duplication
- safe lifecycle handling
- data flow
- null-safety
- cleanup
- object ownership

Codex must not independently finalize design decisions such as:

- skill balance
- damage numbers
- progression rates
- resource costs
- boss pattern design
- character abilities
- lore
- story
- canon relationships
- visual identity

unless explicitly requested to propose options.

When proposing options, label them as proposals.


---

## 25. Visual and VFX Work

When implementing code related to sprites, animation, character visuals, or VFX:

- preserve the project's documented visual canon
- do not invent accessories, weapons, auras, costume elements, or visual effects
- do not redesign a character while performing unrelated technical work
- keep character body/sprite logic separate from combat VFX when the project specification requires separation

If the required visual source is unavailable, report that visual verification remains pending.


---

## 26. Performance

Do not prematurely optimize without evidence.

However, avoid obvious recurring runtime problems such as:

- repeated expensive component searches every frame
- unnecessary allocations in hot combat loops
- uncontrolled Instantiate/Destroy loops for frequently reused combat objects
- duplicated polling when event-driven or owned-state solutions already exist
- repeated hierarchy traversal for stable references

When optimizing, preserve gameplay behavior first.


---

## 27. Comments and Documentation

Comments should explain:

- non-obvious ownership
- unusual constraints
- reasons for important architectural decisions
- lifecycle requirements
- safety assumptions

Do not fill files with comments that simply restate the code.

When creating public reusable systems, use clear names before relying on explanatory comments.


---

## 28. Completion Standard

A task is not complete merely because code was written.

Before reporting completion:

1. review the diff
2. check for unrelated modifications
3. check for obvious compile errors from source inspection
4. run available tests where possible
5. run available compilation where possible
6. verify cleanup paths
7. verify null/reference handling where relevant
8. verify that the implementation follows project documentation
9. identify anything requiring local Unity verification

Final report should distinguish:

- implemented
- verified
- not verified
- unresolved


---

## 29. Default Behavior When Unsure

When uncertain:

- inspect more relevant code
- inspect relevant documentation
- preserve existing behavior
- avoid destructive changes
- avoid inventing specifications
- report ambiguity

Do not silently guess when the guess would become part of the project's architecture,
game design, balance, or canon.
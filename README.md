# x4 Optimizer

Aplica o modo "grafico de batata" no Roblox editando o `GlobalBasicSettings_13.xml`
(sem FastFlags, sem injecao). Faz backup do original e permite restaurar.

## Compilar no GitHub
1. Suba estes arquivos para um repositorio.
2. Aba **Actions** > workflow **Build x4 Optimizer** (roda sozinho a cada push).
3. Abra a execucao e baixe o artefato **x4Optimizer-win-x64** (contem o `x4Optimizer.exe`).

## Uso
- `x4Optimizer.exe` abre o menu.
- `x4Optimizer.exe apply` aplica o modo batata.
- `x4Optimizer.exe restore` restaura as configuracoes originais.

A logica de edicao do XML e baseada no projeto 66mods (MIT) - veja `LICENSE-66mods.txt`.

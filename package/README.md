# Recipe Book

**[English](#english) · [Português](#português)**

---

## English

A read-only book of every recipe you have unlocked. See what each one costs, where it is made, what it takes to upgrade it, and which recipes you unlocked most recently. Nothing is crafted from it: it is only for checking.

### Features

- Press **I** (changeable) to open the book. Press **I** or **Esc** to close it. While it is open you stand still, the mouse is free and the mouse wheel scrolls the list.
- **Only recipes you know.** Nothing you have not unlocked yet is shown.
- **All** tab: recipes grouped by stage of the game, from Meadows to Ashlands (before / after each boss), and by name inside each group.
- **Recent** tab: the newest recipes first, with the in-game day you unlocked them. The mod starts recording when you install it: recipes you already knew have no date and come after the dated ones.
- **Search box:** finds recipes by their name or by a material they use.
- **Stage buttons:** Meadows, Black Forest, Swamp, Mountain, Plains, Mistlands, Ashlands (only the stages you have recipes for).
- **Metal buttons:** Copper, Tin, Bronze, Iron, Silver, Black Metal, Flametal... (only metals your recipes use): shows the recipes that use that metal.
- **Filter button:** shows only one kind of item: Weapons, Armor, Tools, Food, Metals... (the same kinds as chest tags in [Hoardkeeper](https://thunderstore.io/c/valheim/p/DJAventuras/Hoardkeeper/)).
- **Click a recipe** to see what it costs, the crafting station and its level, the cost of each upgrade, and how many of each material you carry.
- **Click a material** (in a recipe or on a metal button) to list the recipes of yours that use it, and the recipe that makes it, if you know one.

### Requirements

- [BepInExPack Valheim](https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/)
- Client-side. Other players and the server don't need it.

### Installation

**r2modman / Thunderstore Mod Manager:** click *Install*. To install a downloaded zip: Settings → *Import local mod*.

**Manual:**
1. Extract the zip.
2. Copy the `plugins/RecipeBook` folder into `Valheim/BepInEx/plugins/`.
   Result: `Valheim/BepInEx/plugins/RecipeBook/RecipeBook.dll`.
3. Launch the game.

To update, delete the old `RecipeBook` folder before copying the new one.

### Configuration

Press **F1** in game (requires Configuration Manager) or edit `BepInEx/config/felih.RecipeBook.cfg`.

| Option | Default | What it does |
|---|---|---|
| Key | I | Opens and closes the recipe book. |
| Scale | 1 | Size of the window (0.6 to 2). |

### Notes

- Client-side: other players and the server do not need it.
- The order in which you unlock recipes is saved on your character, so it follows the character, not the computer.

---

## Português

Um livro, só para consulta, com todas as receitas que você já desbloqueou. Veja quanto custa cada uma, onde ela é feita, o que precisa para melhorá-la e quais você desbloqueou mais recentemente. Nada é fabricado por ali: é só para conferir.

### Recursos

- Aperte **I** (dá para mudar) para abrir o livro. **I** ou **Esc** fecham. Com o livro aberto você fica parado, o mouse fica livre e a rodinha rola a lista.
- **Só receitas que você conhece.** Nada que você ainda não desbloqueou aparece.
- Aba **All**: receitas agrupadas por fase do jogo, de Meadows a Ashlands (antes / depois de cada chefe), e por nome dentro de cada grupo.
- Aba **Recent**: as receitas mais novas primeiro, com o dia do jogo em que você as desbloqueou. O mod começa a registrar quando você o instala: receitas que você já conhecia ficam sem data e vêm depois das datadas.
- **Busca:** acha receitas pelo nome ou por um material que elas usam.
- **Botões de fase:** Meadows, Black Forest, Swamp, Mountain, Plains, Mistlands, Ashlands (só as fases em que você tem receitas).
- **Botões de metal:** Copper, Tin, Bronze, Iron, Silver, Black Metal, Flametal... (só os metais que suas receitas usam): mostra as receitas que usam aquele metal.
- **Botão Filter:** mostra só um tipo de item: Weapons, Armor, Tools, Food, Metals... (os mesmos tipos das tags de baú do [Hoardkeeper](https://thunderstore.io/c/valheim/p/DJAventuras/Hoardkeeper/)).
- **Clique numa receita** para ver quanto custa, a bancada e o nível dela, o custo de cada melhoria e quanto você tem de cada material.
- **Clique num material** (numa receita ou num botão de metal) para listar as receitas suas que o usam, e a receita que o fabrica, se você conhecer uma.

### Requisitos

- [BepInExPack Valheim](https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/)
- Só no cliente. Os outros jogadores e o servidor não precisam do mod.

### Instalação

**r2modman / Thunderstore Mod Manager:** clique em *Install*. Para instalar um zip baixado: Settings → *Import local mod*.

**Manual:**
1. Extraia o zip.
2. Copie a pasta `plugins/RecipeBook` para `Valheim/BepInEx/plugins/`.
   Resultado: `Valheim/BepInEx/plugins/RecipeBook/RecipeBook.dll`.
3. Abra o jogo.

Para atualizar, apague a pasta `RecipeBook` antiga antes de copiar a nova.

### Configuração

Aperte **F1** no jogo (precisa do Configuration Manager) ou edite `BepInEx/config/felih.RecipeBook.cfg`. As opções estão na tabela em inglês acima.

### Observações

- Só no cliente: os outros jogadores e o servidor não precisam do mod.
- A ordem em que você desbloqueia as receitas fica gravada no seu personagem, então acompanha o personagem, não o computador.

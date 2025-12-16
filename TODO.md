# BaconBinary.ObjectEditor - Roadmap & TODOs

## 1. Prioridade Crítica (Fundação da Edição)
- [x] **Refatorar `ThingType` para `INotifyPropertyChanged`:** Implementado com `ObservableObject`.
- [x] **Drag & Drop:**
    - [x] Implementar `DragDrop` da Paleta de Sprites para os Slots do Canvas.
    - [x] Atualizar o `ThingType` em memória ao soltar um sprite.
    - [x] **BUGFIX:** Corrigir renderização embaralhada de Outfits no canvas de composição.
    - [ ] **BUGFIX:** Corrigir renderização embaralhada de Items no canvas de composição.
- [ ] **Redimensionamento e Patterns:**
    - [x] Habilitar os `NumericUpDown` de Width, Height, Layers e Patterns.
    - [x] **VALIDAÇÃO:** Limitar valores de size/pattern entre 0 e 255.
    - [ ] Implementar lógica para redimensionar a matriz de sprites ao alterar esses valores.
- [x] **Save Changes (Memória) & Dirty State:**
    - [x] Implementar lógica para o botão "Save Changes" na tela de edição.
    - [ ] Adicionar indicador visual (*) quando um item foi modificado.
  - [ ] **Prompt de Formato:** Ao clicar em compile ou compile as..., pedir para selecionar entre dat/spr e asset/meta.
    - Ao clicar no botão de compile ou compile as... deve aparecer um prompt, pedindo para selecionar entre dat/spr e asset/meta.
      - dat/spr segue o padrão do tibia
      - asset/metadata adiciona algumas informações no header:
        - header "BSUIT"
        - os metadata reader devem se adaptar a, se for um .meta ler esses dados antes (pode adicionar no ClientFeatures.IsNewVersion)
        - Vamos reorganizar esses dois novos formatos para serem mais leves, e rapidos de ler/escrever (tenha liberdade)
          - o .asset pode ser dividido em N partes (.asset0, .asset1, ..., .assetN)
          - preciso de um arquivo de especificação, pois vamos ter que portar isso para c++, para o OTCLIENT ler, faça um arquivo .md explicando para a IA como implementar o .asset e .meta

## 2. Prioridade Alta (Funcionalidade de Arquivo e Criação)
- [x] **Compilação (Save Project):**
    - [x] Implementar `DatWriter` no Core.
    - [x] Implementar `SprWriter` no Core (Carregamento em memória).
    - [x] Conectar botão "Compile Project".
- [ ] **Criação e Clonagem:**
    - **Clonar Objeto:** Botão direito -> Clone. Cria cópia com novo ID no final da lista.
    - **Novo Objeto:** Botão "New Item". Cria item vazio na categoria atual.
- [ ] **Importação de Imagens:**
    - Permitir importar PNG/BMP.
    - Fatiar automaticamente em blocos 32x32.
    - Adicionar novos sprites ao final do arquivo `.spr` virtual.

## 3. Prioridade Média (Usabilidade e Compatibilidade)
- [ ] **Paginação na MainView:**
    - Paginar a lista principal de itens.
    - Adicionar seletor de "Itens por Página" (100, 200, 500, 1000).
- [ ] **Painel de Sprites (MainView):**
    - Implementar a aba "SPRITES" na direita da tela principal.
    - Mostrar lista de sprites paginada.
- [ ] **Serviço de Diálogo:**
    - Criar um serviço para mostrar diálogos de confirmação (ex: "Salvar alterações?").
    - Implementar o prompt de salvamento ao trocar de item no editor.
- [ ] **Compatibilidade Browser (WASM):**
    - **Leitura via Stream:** Refatorar `DatReader` e `SprReader` para aceitar `System.IO.Stream`.
- [ ] **Hotkeys:** Garantir que atalhos como Ctrl+O, Ctrl+S funcionem globalmente.
- [ ] **Seleção de Frame Group:**
    - Habilitar seleção apenas para **Outfits**.
    - Permitir criar/remover Frame Groups.

## 4. Prioridade Baixa (Extras e Polimento)
- [ ] **Formatos Externos:**
    - Suporte a leitura/escrita de `.obd` (Object Builder).
    - Suporte a `.basset` (Formato proprietário com metadados).
- [ ] **Sprite Sheet:**
    - Exportar objeto selecionado como imagem única.
    - Importar sprite sheet para substituir animações.
- [ ] **Suporte a Versões Recentes:** Implementar leitura/escrita para clientes 11+ (Protobuf, Criptografia).
- [ ] **Filtros Avançados:** Implementar busca por propriedades na barra de pesquisa.
- [ ] **Visualização de Animações:** Garantir que o preview na lista principal (MainView) anime suavemente.
- [ ] **Melhorias Visuais:** Refinar o layout e feedback visual da tela de edição.

---
### Status Atual
- [x] **Implementar EditorView:** Layout base, paginação, painel de propriedades dinâmico.
- [x] **Canvas de Composição:** Renderização correta da grade de slots.
- [x] **Controles de Animação e Direção:** Play/Pause, Slider, Setas.
- [x] **Navegação:** Entrar/Sair do modo de edição.
- [x] **Edição em Memória:** Sistema de `TempProps` para edição não destrutiva.
- [x] **Persistência de Sessão:** Salva e pré-carrega o último projeto.
- [x] **ThingType Reativo:** Refatorado para `ObservableObject`.
- [x] **Drag & Drop:** Implementado e funcional.
- [x] **Leitura/Escrita DAT/SPR:** Implementada.

# TODO - Item Editor Implementation

- [X] Implement `ItemsXmlReader` and `ItemsXmlWriter` to handle all unique attributes from `items.xml`.
- [X] Create `ItemField` class and add it to `ServerItem`.
- [X] Instantiate OTB/XML readers and writers in `MainViewModel`.
- [X] Add an optional loading mechanism for `.otb` and `.xml` files in `MainView`/`MainViewModel`.
- [X] Create the `ItemEditorView` and `ItemEditorViewModel`.
    - [X] Basic window and ViewModel structure.
    - [X] Item list with pagination on the right.
    - [X] OTB flags/attributes editor next to the list.
    - [X] XML properties editor.
- [X] Implement the "Open Item Editor" functionality in `MainView` to launch the new window.
- [X] Implement the right-click context menu in `MainView` for "Create New Server Item" and "Go to Server Item".
- [X] Ensure consistent styling and color scheme.
- [X] Implement the existing popup/alert system for user notifications.

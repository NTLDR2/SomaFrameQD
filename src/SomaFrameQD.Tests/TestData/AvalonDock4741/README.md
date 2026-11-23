# AvalonDock 4.74.1 complete-state fixture

**Intent:** preserve an immutable pre-v5 SomaFrameQD whole-state input so future
docking migrations must prove forward import and binary rollback without
rewriting the only compatible source state.

`ApplicationState.bin` was written from SomaFrameQD commit
`15cdb9afad39c7fd8ddaf732b426f9eb9c21719e` with AvalonDock 4.74.1. It contains
SomaFrameQD's binary item envelope and payloads followed by the serializer's XML for
two documents and six tools spanning docked, floating, and hidden state.

SHA-256:
`0FF5F1CD3C57BC6AA05E3583A54AB6B42CE976D6F9C0E3BFBA1C3AEFFA1FC9A9`.

Do not modify or regenerate this file. Add a separate writer-version directory
for future fixtures.

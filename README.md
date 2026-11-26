# Kısayol

Photoshop ve Illustrator kısayollarını gösteren küçük bir Windows uygulaması.

- Türkçe arama: "katman kopyala" yazınca Ctrl+J çıkar.
- Akıllı mod: Photoshop öndeyken Photoshop listesi, Illustrator öndeyken Illustrator listesi açılır.
- Ctrl+Alt+K ile her yerden aç/gizle, istenirse hep üstte durur.

## Kaynaklar
- Photoshop menü kısayolları: Photoshop 2027'nin kendi `Default Keyboard Shortcuts.kys` dosyası.
- Photoshop araç tuşları: Photoshop'un varsayılan araç tuşları.
- Illustrator: Adobe'nin resmi "Default keyboard shortcuts for Illustrator" sayfası (Windows sütunu).

## Derleme
1. `python tools/gen.py` (data/*.tsv → web/data.js)
2. `dotnet publish src/Kisayol -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -o publish/win-x64`
3. `ISCC installer/kisayol.iss` → `release/Kisayol-Setup-<sürüm>.exe`

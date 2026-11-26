# Builds src/Kisayol/web/data.js from data/*.tsv
import io, json, os
root = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
out = {}
for app in ("ps", "ai"):
    rows = []
    lines = io.open(os.path.join(root, "data", app + ".tsv"), encoding="utf-8").read().splitlines()
    for l in lines[1:]:
        if not l.strip(): continue
        c, en, tr, k, s = l.split("\t")
        rows.append({"c": c, "en": en, "tr": tr, "k": k.replace(" + ", "+")})
    out[app] = rows
js = "window.KISAYOL = " + json.dumps(out, ensure_ascii=False, indent=0) + ";\n"
io.open(os.path.join(root, "src", "Kisayol", "web", "data.js"), "w", encoding="utf-8").write(js)
print({k: len(v) for k, v in out.items()})

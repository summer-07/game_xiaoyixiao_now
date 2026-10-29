"""Subset the game font to the characters the C# sources use (plus ASCII) to keep the mini-game package small.

Usage: python3 Tools/subset_font.py <full-font.ttf>
Writes Assets/_Project/Resources/Fonts/GameFont.ttf. Re-run whenever Texts.cs gains new characters.
Source font: ZCOOL KuaiLe (SIL OFL 1.1), https://github.com/google/fonts/tree/main/ofl/zcoolkuaile
"""
import glob
import sys

from fontTools import subset

ROOT = "Assets/_Project"
OUTPUT = f"{ROOT}/Resources/Fonts/GameFont.ttf"


def used_characters():
    text = "".join(open(p, encoding="utf8").read() for p in glob.glob(f"{ROOT}/Scripts/**/*.cs", recursive=True))
    ascii_printable = "".join(chr(c) for c in range(32, 127))
    return "".join(sorted(set(c for c in text if ord(c) > 127) | set(ascii_printable) | set("…！？，。：、×")))


def main(source):
    options = subset.Options()
    options.layout_features = ["*"]
    font = subset.load_font(source, options)
    subsetter = subset.Subsetter(options)
    subsetter.populate(text=used_characters())
    subsetter.subset(font)
    subset.save_font(font, OUTPUT, options)
    print(f"wrote {OUTPUT}")


if __name__ == "__main__":
    main(sys.argv[1])

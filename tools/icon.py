from PIL import Image, ImageDraw, ImageFont
S=256
im=Image.new("RGBA",(S,S),(0,0,0,0)); d=ImageDraw.Draw(im)
d.rounded_rectangle((8,8,S-8,S-8),radius=52,fill=(20,21,26,255))
# keycap
d.rounded_rectangle((44,58,S-44,S-44),radius=30,fill=(13,14,18,255))
d.rounded_rectangle((44,50,S-44,S-58),radius=30,fill=(38,41,51,255),outline=(90,169,255,255),width=6)
try: f=ImageFont.truetype("C:/Windows/Fonts/segoeuib.ttf",120)
except: f=ImageFont.load_default()
d.text((S/2,(50+S-58)/2),"K",font=f,fill=(233,234,240,255),anchor="mm")
d.rectangle((60,S-34,S-60,S-26),fill=(255,165,61,255))
im.save("src/Kisayol/app.ico",sizes=[(16,16),(24,24),(32,32),(48,48),(64,64),(128,128),(256,256)])
im.save("tools/icon.png")

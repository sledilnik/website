# Podatki o širjenju bolezni COVID-19 v Sloveniji

*Projekt zbira, analizira in objavlja podatke o širjenju koronavirusa SARS-CoV-2, ki povzroča bolezen COVID-19 v Sloveniji. Javnosti želimo omogočiti čim boljši pregled nad razsežnostjo težave in pravilno oceno tveganja.*

## Zakaj?

Pravilno zbrani in ažurno ter transparentno objavljeni podatki so po izkušnjah držav, kjer jim je virus uspelo najbolj zajeziti, kritičnega pomena za učinkovit odziv sistemov javnega zdravja. Šele tako objavljeni podatki so temelj za razumevanje dogajanja, aktivno samozaščitno ravnanje ljudi ter sprejemanje nujnosti ukrepov.

Podatke zbiramo iz različnih javno dostopnih virov, od sobote, 28. marca dalje pa imamo vzpostavljeno tudi direktno povezavo z zdravstvenimi zavodi in [NIJZ](https://www.nijz.si/). Ti nam pošiljajo strukturirane podatke, ki jih potem validiramo in oblikujemo v obliko, primerno za vizualizacije in  predstavitev javnosti, kakor tudi za nadaljne delo pri razvoju modelov in napovedi. Ker so podatki iz medijev in nekaterih drugih virov kdaj tudi nejasni in nedosledni, [preglednica](https://tinyurl.com/sledilnik-gdocs) vključuje tudi opombe o virih in sklepanju na podlagi nepopolnih podatkov.

## Pomagajte tudi vi – nam, sebi in drugim

Projekt je z zbiranjem podatkov začel [Luka Renko](https://twitter.com/LukaRenko), sedaj pa v njem prostovoljno in dejavno sodeluje od 20 do 45 ljudi, saj vnašanje in preverjanje podatkov ter programiranje zahteva vedno več pozornosti. Projekt nastaja z množičnim prostovoljnim sodelovanjem (t.i. "crowdsourcing"), kjer lahko vsak prispeva po svojih močeh.

Imaš idejo za nov prikaz, bi urejal/a podatke ali pomagal/a pri kodiranju vizualizacij? Javi se nam na [info@sledilnik.org](mailto:info@sledilnik.org) pa se pridruži naši [Slack skupnosti](https://sledilnik.slack.com). Pridruži se in pomagaj!


## Zbrani podatki

Vključeni so naslednji dnevni podatki (z zgodovino) iz [NIJZ in različnih javnih virov](/sl/data):

- število opravljenih testov in število potrjeno okuženih
- število potrjeno okuženih po kategorijah: po starosti, spolu, regijah in občinah
- evidenca o bolnišnični oskrbi pacientov s COVID-19: hospitalizirani, v intenzivni enoti, kritično stanje, odpuščeni iz bolniške oskrbe, ozdraveli
- spremljanje posameznih primerov, še zlasti v kritičnih dejavnostih: zaposleni v zdravstvu, domovih starejših občanov, civilni zaščiti
- zmogljivost zdravstvenega sistema: število postelj, enot intenzivne nege, respiratorjev za predihavanje ...

Sproti se trudimo dodajati tudi nove pomembne kategorije.

Vsi podatki so na voljo v obliki [**CSV datotek, REST API-ja in Google Docs preglednic**](/sl/data).


## Uporaba podatkov

Naše podatke že uporabljajo tudi drugi portali; veseli bomo, če se jim boste pridružili.

**Pozor**: Informacije, objavljene na naši spletni strani, vključno s povezavami na modele in druge strani, s katerimi nismo neposredno povezani, so pripravljene z največjo mogočo skrbnostjo ob uporabi razpoložljivih virov podatkov, znanja, metodologij in tehnologij, upoštevajoč znanstvene standarde.
Verjamemo, da lahko prikazi in modeli pomagajo razložiti različne dejavnike širjenja virusa, med drugim tudi vpliv sprejetih in mogočih nadaljnjih ukrepov, s čimer želimo poudariti, da imamo v tej pandemiji vsi pomembno vlogo.
 Kljub temu ne moremo 100 % oz. v celoti zagotoviti točnosti, popolnosti ali uporabnosti informacij na teh spletnih straneh, in izrecno zavračamo kakršno koli odgovornost za nadaljnje interpretacije in simulacije, ki naše prikaze navajajo kot vir.


## Uporaba grafov in vizualizacij

[Naše grafe in prikaze](/sl/stats) lahko na svojo spletni strani uporabite tudi vi. [Vgradite lahko poljuben graf ali prikaz](/sl/embed) in ga prilagodite svoji spletni strani.


## Pogoji uporabe

Uporaba podatkov, grafov in sodelovanje so zaželjeni: podatki so zbrani iz virov v javni domeni in jih lahko prosto uporabljate, urejate, predelujete ali vključujete v vse netržne vsebine ob navedbi vira – [**covid-19.sledilnik.org**](https://covid-19.sledilnik.org/). Če ni določeno drugače, velja za vso vsebino na tej strani licenca Creative Commons: [Priznanje avtorstva-Deljenje pod enakimi pogoji 3.0](https://creativecommons.org/licenses/by-sa/3.0/deed.sl).

Za izvoz podatkov v drugih oblikah, uporabo za vizualizacije ali druge oblike sodelovanja nas kontaktirajte na info@sledilnik.org.

Uporaba Sledilnika ne bi bila možna brez spletnega servisa CloudFlare, preko katerega pretakamo podatke in ki za svoje nemoteno delovanje na vaš računalnik vstavi en sam piškotek, ki tam ostane največ 30 dni. Več informacij o varovanju zasebnosti [najdete na njihovi spletni strani]([https://support.cloudflare.com/hc/en-us/articles/200170156-Understanding-the-Cloudflare-Cookies#12345682](https://support.cloudflare.com/hc/en-us/articles/200170156-Understanding-the-Cloudflare-Cookies#12345682).



## Ekipa

COVID-19 Sledilnik je odprto podatkovni in odprto kodni projekt, ki ne bi bil mogoč brez številnih vnosov in komentarjev v bazo bolj ali manj anonimnih posameznikov, sodelavcev, znancev, kolegov ter izmenjave idej skupnosti na Slacku. HVALA!

Sodelavci projekta:

-   [Aleks Jakulin](https://twitter.com/aleksj) - modeliranje, viri
-   [Ana Slavec](https://twitter.com/aslavec) - ankete, analize, vsebine
-   [Andraž Vrhovec](https://github.com/overlordtm) - spletna stran, infrastuktura
-   [Andrej Srakar](http://www.ier.si/) - viri, modeliranje
-   [Andrej Viršček](https://udomacenastatistika.wordpress.com/author/vandrej/) - modeliranje
-   [Anže Voh Boštic](https://podcrto.si/author/anze/) - viri, validacija virov, modeliranje
-   [Barbara Krajnc](https://twitter.com/bakrajnc) - vsebine, komunikacija
-   [Bojan Košir](https://twitter.com/BojanKosir) - podatkovni model in validacija, viri
-   [Boštjan Špetič](https://www.igzebedze.com/) - podatkovno skrejpanje, komunikacija
-   [Darja Potočan](http://www.marsowci.net/) - družabna omrežja, viri
-   [Demjan Vester](https://github.com/VesterDe) - spletna stran
-   [Eva Matjašič](https://github.com/Blonduos/) - spletna stran
-   [Gašper Mramor](https://www.linkedin.com/in/gaspermramor) - podatkovni model in validacija, viri
-   [Grega Milčinski](https://www.linkedin.com/in/gregamilcinski/) - viri, validacija podatkov
-   [Igor Brejc](https://twitter.com/breki74) - vizualizacije
-   [Jaka Daneu](https://github.com/jalezi) -  spletna stran
-   [Jana Javornik](https://twitter.com/JanaSvenska) - vsebine, viri
-   [Janez Gorenc](https://si.linkedin.com/in/janez-gorenc-03415868) - prevodi
-   [Janez Žibert](https://pacs.zf.uni-lj.si/janez-zibert/) - koordinacija za modeliranje, SEIR model, modeliranje
-   [Joh Dokler](https://github.com/joahim) - vizualizacije, spletna stran
-   [Jure Novak](http://jurenovak.org/) - tekstopisje, viri
-   [Jure Sobočan](https://www.linkedin.com/in/juresobocan) - logo, grafično oblikovanje
-   [Jurij Bajželj](https://www.linkedin.com/in/bajzelj) - podatkovni model, validacija, vključevanje uporabnikov
-   [Luka Medic](https://www.facebook.com/luka.medic.79) - modeliranje, validacija podatkov, covid-spark.info
-   [Luka Renko](https://twitter.com/lukarenko) - koordinacija, podatkovni model in validacija
-   [Maja Pohar Perme](http://ibmi.mf.uni-lj.si/sl/o-ibmi/osebje) - modeliranje
-   [Maja Založnik](https://www.linkedin.com/in/maja-zalo%C5%BEnik-26034a84) - koordinacija, podatkovni model in validacija, viri
-   [Maks Mržek](https://www.linkedin.com/in/maks-mr%C5%BEek-98798066/) - validacija podatkov, vizualizacije
-   [Marko Brumen](https://twitter.com/multikultivator) - družabna omrežja, spletna stran, viri
-   [Maruša Gorišek](https://www.linkedin.com/in/marusagorisek/) - spletna stran, viri
-   [Matej Aleksov](https://www.linkedin.com/in/matej-aleksov/) - družabna omrežja, grafično oblikovanje
-   [Matej Jurko](https://www.linkedin.com/in/matejjurko/) - viri, validacija podatkov
-   [Matej Meglič](https://www.linkedin.com/in/matejmeglic/) - koordinacija, podatkovni model in validacija
-   [Matjaž Drolc](https://twitter.com/MatjazDrolc/) - vizualizacije
-   [Matjaž Lipuš](https://twitter.com/MatjazL) - viri, validacija podatkov
-   [Mia Erbus](https://github.com/miaerbus) - spletna stran
-   [Miha Kadunc](https://twitter.com/miha_kadunc) - podatkovni model in validacija, modeliranje
-   [Miha Markič](https://twitter.com/MihaMarkic) - REST API
-   [Milo Ivir](https://hosted.weblate.org/user/milotype/) - prevodi
-   [Mitja Potočin](https://github.com/mitjapotocin) - spletna stran
-   [Nace Štruc](https://www.nace.si/) - vizualizacije
-   [Nejc Davidović](https://twitter.com/NejcDavidovic) - tekstopisje, viri
-   [Nina Rolih](https://www.facebook.com/tanoranina) - vsebine, grafično oblikovanje
-   [Peter Keše](https://twitter.com/pkese/) - vizualizacije
-   [Pika Založnik]() - vsebine, viri
-   [Roman Luštrik](https://www.linkedin.com/in/roman-lu%C5%A1trik-5a6586ab) - skriptanje, statistika
-   [Sabina Tamše Kozovinc](https://www.linkedin.com/in/sabina-tamse-copywriter/) - tekstopisje, viri, FAQ
-   [Sebastian Pleško](https://twitter.com/seba1337) - viri
-   [Sebastjan Cizel](https://sebastjancizel.github.io) - vizualizacije
-   [Štefan Baebler](https://www.linkedin.com/in/stefanbaebler/) - spletna stran, zemljevid, prevodi
-   [Tanja Ambrožič]() - validacija podatkov
-   [Tomaž Kovačič](https://www.linkedin.com/in/tomazkovacic) - vizualizacije
-   [Vanja Cvelbar](https://github.com/b100w11) - prevodi
-   [Vladimir Nesković](https://www.linkedin.com/in/k35m4/) - podatkovni model, modeliranje
-   [Zarja Muršič](https://twitter.com/piskotk) - modeliranje, validacija, viri, komunikacija
-   [Žiga Brenčič](https://zigabrencic.com) - modeliranje
-   [Žiga Zaplotnik](https://twitter.com/ZaplotnikZiga) - mrežni model, modeliranje

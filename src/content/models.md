# Modeli in napovedi

V naši skupnosti sodelujejo tudi strokovnjaki za statistično modeliranje in računalniške simulacije.
Na tej strani objavljamo povezave na nekatere od njihovih modelov,
ki so jih pripravili in umerili z uporabo podatkov [COVID-19 Sledilnik](https://covid-19.sledilnik.org).

Modeli v veliki meri upoštevajo doslej znane informacije o bolezni COVID-19 in njenem širjenju v Sloveniji, a kljub temu **ne ponujajo natančnih napovedi** za prihodnji potek epidemije, **zato je potrebno skrbno prebrati vse predpostavke modela**.
Podatki o testih in potrjenih okuženih osebah v Sloveniji so skopi, zato sta oba predstavljena modela umerjena na podatke o hospitalizacijah. Pomanjkanje natančnih podatkov o primerih je eden glavnih razlogov za nedoločenosti.

Svetovna znanost vlaga velike napore v boj z boleznijo COVID-19, a vendar mnogi vidiki širjenja in razvoja bolezni še niso raziskani. Še posebno velika je negotovost glede učinka ukrepov, ki jih vlade po svetu izvajajo za omejitev bolezni. Poleg tega je, zaradi časovnih zamikov med okužbo in potrditvijo, praktično nemogoče točno oceniti dejansko stanje okuženosti in hitrost širjenja okužbe v populaciji. Vse to so razlogi, da so modelom pripadajoči intervali nedoločenosti sorazmerno veliki, in, dlje kot gledamo v prihodnost, hitreje rastejo.

## Model prenosa virusa po socialnem omrežju prebivalcev Slovenije
V sodelovanju z [Žigom Zaplotnikom](https://twitter.com/ZaplotnikZiga) s Fakultete za matematiko in fiziko Univerze v Ljubljani smo pripravili verjetnostno napoved poteka pandemije v Sloveniji. V simulaciji se virus prenaša po realističnem modelu socialnega omrežja Slovencev, ki vsebuje več kot 2 milijona vozlišč (1 za vsakega prebivalca Slovenije), razdeljenih v gospodinjstva in domove oskrbovancev. Vozlišča naključno povežemo tudi izven teh enot, glede na znane porazdelitve kontaktov – nekatere  osebe imajo dnevno več kontaktov, druge manj. To omogoča, da lahko z modelom efektivno simuliramo različne strategije zajezitve virusa.Verjetnostno napoved dobimo tako, da pripravimo množico simulacij z rahlo spremenjenim začetnim pogojem in parametri, ki določajo širjenje koronavirusa in potek bolezni COVID-19. Ta se tudi med posameznimi osebami razlikuje. Podrobnejši opis modela se nahaja [v tem delovnem dokumentu](https://arxiv.org/pdf/2005.13282.pdf), zgodovina izračunov pa [tule](https://fiz.fmf.uni-lj.si/~zaplotnikz/korona/).

<a href="https://fiz.fmf.uni-lj.si/~zaplotnikz/korona/last_forecast/potek_pandemije.png" class="img-link">
<img alt="Omrežje model" src="https://fiz.fmf.uni-lj.si/~zaplotnikz/korona/last_forecast/potek_pandemije.png"></a>


## SEIR model
V sodelovanju s [prof. Janezom Žibertom](https://pacs.zf.uni-lj.si/janez-zibert/) z Zdravstvene fakultete, Univerze v Ljubljani smo pripravili model SEIR (Susceptible, Exposed, Infected, and Recovered) s podmodeli za modeliranje bolnišničnih obravnav, obravnav na intenzivni negi in smrti, ki ima parametre usklajene s podatki o hospitalizacijah in klinično sliko COVID-19 v Sloveniji.

Zadnje simulacije modela si lahko ogledate  [na spletni strani](https://pacs.zf.uni-lj.si/shinyR/apps/projects/CoronaSim/). 
Model lahko tudi [sami preizkusite](https://pacs.zf.uni-lj.si/shinyR/apps/projects/CoronaSim5/) tako, da spreminjate posamezne parametre in vidite njihov vpliv na razvoj epidemije. 

<a href="https://pacs.zf.uni-lj.si/public/coronasim/zadnja-simulacija.png" class="img-link">
<img alt="SEIR model" src="https://pacs.zf.uni-lj.si/public/coronasim/zadnja-simulacija.png"></a>

## Ocenjevanje stopnje reprodukcije
Skupina [Inštituta za biostatistiko in medicinsko informatiko](http://ibmi.mf.uni-lj.si/) Medicinske fakultete Univerze v Ljubljani je pod vodstvom prof. Maje Pohar Perme na podlagi opaženih podatkov ocenila stopnjo reprodukcije, to je hitrost širjenja okužbe, med posamičnimi intervencijami. Pri modeliranju so uporabili tehnike Bayesovske statistike, ki omogoča ocenjevanje kompleksnih parametrov pri omejenem številu podatkov, kar nudi možnost hitrejšega odziva.

Podrobnejši opis prvotne metodologije in izsledkov je objavljen kot članek v [Zdravniškem vestniku](https://vestnik.szd.si/index.php/ZdravVest/article/view/3068).

Prvotni model je bil kasneje nadgrajen in se sedaj dnevni rezultati objavljajo na [tej povezavi](https://oblak8.mf.uni-lj.si/covid19/).

<a href="http://ibmi.mf.uni-lj.si/files/Pregledni%20povzetek_74e.pdf" class="img-link">
<img alt="R_t model" src="https://stat.columbia.edu/~jakulin/Covid/ocene_rt.png"></a>


## Srečanje slovenskih znanstvenikov na temo COVID-19 ukrepov 

Mlada sekcija Statističnega društva Slovenije je 21. aprila 2020 organizirala spletni pogovor, ki ga je gostil Inštitut za biostatistiko in medicinsko informatiko (IBMI) Medicinske fakultete Univerze v Ljubljani. Pogovor sta vodila dr. Andrej Srakar in dr. Ana Slavec. STA je dogodek prenašala v živo več kot 850 udeležencem. Posnetek in predavanja si lahko [preberete tukaj](https://medium.com/sledilnik/64233b35580c).  


## Drugi modeli za Slovenijo

-  [Dr. Matjaž Leskovar](http://r4.ijs.si/leskovar) z Odseka za reaktorsko tehniko Instituta Jožef Stefan pripravlja napoved razvoja epidemije v Sloveniji glede na javno dostopne podatke o potrjenih okužbah in trenutni zasedenosti bolnišnic. Na voljo je [opis modela, s povezavo na izvorno datoteko](http://r4.ijs.si/COVID19model), [arhiv napovedi](http://r4.ijs.si/COVID19arhiv) in [prikaz zadnjih rezultatov](http://r4.ijs.si/COVID19).

-  [Dr. Andrej Srakar](https://sites.google.com/site/andrejsrakar1975/) je za blog Udomačena statistika napisal izvrsten pregledni članek [**Uvod v modeliranje in statistične vidike COVID-19**](https://udomacenastatistika.wordpress.com/2020/04/20/uvod-v-modeliranje-in-statisticne-vidike-covid-19/) o ključnih modelih širjenja epidemije bolezni COVID-19, ki so nastali v Sloveniji.

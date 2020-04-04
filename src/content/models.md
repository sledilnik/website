# Modeli in napovedi

V naši skupnosti sodelujejo tudi strokovnjaki za statistično modeliranje in računalniške simulacije.
Na tej strani objavljamo povezave na nekatere od njihovih modelov,
ki so jih pripravili in umerili z uporabo podatkov [COVID-19 Sledilnik](covid-19.sledilnik.org).

Modeli v veliki meri upoštevajo doslej znane informacije o bolezni COVID-19 in njenem širjenju v Sloveniji, a kljub temu **ne ponujajo natančnih napovedi** za prihodnji potek epidemije, **zato je potrebno skrbno prebrati vse predpostavke modela**.
Podatki o testih in potrjenih okuženih osebah v Sloveniji so skopi, zato sta oba predstavljena modela umerjena na podatke o hospitalizacijah. Pomanjkanje natančnih podatkov o primerih je eden glavnih razlogov za nedoločenosti.

Svetovna znanost vlaga velike napore v boj z boleznijo COVID-19, a vendar mnogi vidiki širjenja in razvoja bolezni še niso raziskani. Še posebno velika je negotovost glede učinka ukrepov, ki jih vlade po svetu izvajajo za omejitev bolezni. Poleg tega je, zaradi časovnih zamikov med okužbo in potrditvijo, praktično nemogoče točno oceniti dejansko stanje okuženosti in hitrost širjenja okužbe v populaciji. Vse to so razlogi, da so modelom pripadajoči intervali nedoločenosti sorazmerno veliki, in, dlje kot gledamo v prihodnost, hitreje rastejo.

## Model prenosa virusa po socialnem omrežju prebivalcev Slovenije
V sodelovanju z [Žigom Zaplotnikom](https://twitter.com/ZaplotnikZiga) s Fakultete za matematiko in fiziko Univerze v Ljubljani smo pripravili verjetnostno napoved poteka pandemije v Sloveniji. V simulaciji se virus prenaša po realističnem modelu socialnega omrežja Slovencev, ki vsebuje več kot 2 milijona vozlišč (1 za vsakega prebivalca Slovenije), razdeljenih v gospodinjstva in domove oskrbovancev. Vozlišča naključno povežemo tudi izven teh enot, glede na znane porazdelitve kontaktov – nekatere  osebe imajo dnevno več kontaktov, druge manj. To omogoča, da lahko z modelom efektivno simuliramo različne strategije zajezitve virusa.Verjetnostno napoved dobimo tako, da pripravimo množico simulacij z rahlo spremenjenim začetnim pogojem in parametri, ki določajo širjenje koronavirusa in potek bolezni COVID-19. Ta se tudi med posameznimi osebami razlikuje. Podrobnejši opis modela se nahaja [v tem delovnem dokumentu](https://nextcloud.fmf.uni-lj.si/s/AdNLwYoA4JyKFBG), zgodovina izračunov pa [tule](https://fiz.fmf.uni-lj.si/~zaplotnikz/korona/).

[<img src="https://fiz.fmf.uni-lj.si/~zaplotnikz/korona/last_forecast/potek_pandemije.png" width="700">](https://fiz.fmf.uni-lj.si/~zaplotnikz/korona/last_forecast/potek_pandemije.png)


## SEIR model
V sodelovanju s [prof. Janezom Žibertom](https://pacs.zf.uni-lj.si/janez-zibert/) z Zdravstvene fakultete, Univerze v Ljubljani smo pripravili model SEIR (Susceptible, Exposed, Infected, and Recovered) s podmodeli za modeliranje bolnišničnih obravnav, obravnav na intenzivni negi in smrti, ki ima parametre usklajene s podatki o hospitalizacijah in klinično sliko COVID-19 v Sloveniji. 

Model lahko tudi sami [preizkusite](https://pacs.zf.uni-lj.si/shinyR/apps/projects/CoronaSim3/) tako, da spreminjate posamezne parametre in vidite njihov vpliv na razvoj epidemije. 

[<img src="https://covid-19.sledilnik.org/images/20200403-model-jzibert.png" width="700">](https://pacs.zf.uni-lj.si/shinyR/apps/projects/CoronaSim3/)


# Modeli in napovedi

V skupnosti sodelujejo tudi strokovnjaki za statistično modeliranje in računalniške simulacije...
na tej strani objavljamo povezave na nekatere od njihovih modelov, ki so jih pripravili in umerili z uporabo podatkov [COVID-19 Sledilnik](covid-19.sledilnik.org) 

Modeli v veliki meri upoštevajo doslej znane informacije o bolezni COVID-19 in njenem širjenju v Sloveniji, a kljub temu **ne ponujajo natančnih napovedi** za prihodnji potek epidemije, zato je **potrebno skrbno prebrati vse predpostavke modela**.

Svetovna znanost vlaga velike napore v boj z boleznijo COVID-19, a vendar mnogi vidiki širjenja in razvoja bolezni še niso raziskani. Še posebno velika je negotovost glede učinka ukrepov, ki jih vlade po svetu izvajajo za omejitev bolezni. Poleg tega je, zaradi časovnih zamikov med okužbo in potrditvijo, praktično nemogoče točno oceniti dejansko stanje okuženosti in hitrost širjenja okužbe v populaciji. Vse to so razlogi, da so modelom pripadajoči intervali nedoločenosti sorazmerno veliki, in, dlje kot gledamo v prihodnost, hitreje rastejo.

## Model prenosa virusa po socialnem omrežju prebivalcev Slovenije
V sodelovanju z [Žigom Zaplotnikom](https://twitter.com/ZaplotnikZiga) s Fakultete za matematiko in fiziko Univerze v Ljubljani smo pripravili verjetnostno napoved poteka pandemije v Sloveniji. V simulaciji se virus prenaša po realističnem modelu socialnega omrežja Slovencev, ki vsebuje več kot 2 milijona vozlišč (1 za vsakega prebivalca Slovenije), razdeljenih v gospodinjstva in domove oskrbovancev. Vozlišča naključno povežemo tudi izven teh enot, glede na znane porazdelitve kontaktov – nekatere  osebe imajo dnevno več kontaktov, druge manj. To omogoča, da lahko z modelom efektivno simuliramo različne strategije zajezitve virusa.Verjetnostno napoved dobimo tako, da pripravimo množico simulacij z rahlo spremenjenim začetnim pogojem in parametri, ki določajo širjenje koronavirusa in potek bolezni COVID-19. Ta se tudi med posameznimi osebami razlikuje. Podrobnejši opis modela se nahaja [v tem delovnem dokumentu](https://nextcloud.fmf.uni-lj.si/s/AdNLwYoA4JyKFBG), zgodovina izračunov pa [tule](https://fiz.fmf.uni-lj.si/~zaplotnikz/korona/).

<img src="https://covid-19.sledilnik.org/images/20200403-model-zzaplotnik.png" width="700">


## SEIR model
V sodelovanju s [prof. Janezom Žibertom](https://pacs.zf.uni-lj.si/janez-zibert/) iz Zdravstvene fakultete smo pripravili model SEIR (Susceptible, Exposed, Infected, and Resistant), ki ima parametre usklajene s podatki o hospitalizacijah ter klinično sliko COVID-19. Nakazuje na to, da imamo večje število okuženih, ki se jim ne sledi, zato se okužba v populaciji širi naprej.

<img src="https://covid-19.sledilnik.org/images/20200403-model-jzibert.png" width="700">

Model lahko tudi sami v [živo uporabljate](https://pacs.zf.uni-lj.si/shinyR/apps/projects/CoronaSim3/), tako da spreminjate posamezne parametre in vidite vpliv na razvoj epidemije. Klinične in empirične predpostavke so pod podpisom.



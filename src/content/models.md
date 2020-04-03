# Modeli in napovedi

V skupnosti sodelujejo tudi strokovnjaki za statistično modeliranje in računalniške simulacije...
na tej strani objavljamo povezave na nekatere od njihovih modelov, ki so jih pripravili in umerili z uporabo podatkov [COVID-19 Sledilnik](covid-19.sledilnik.org) 

Modeli v veliki meri upoštevajo doslej znane informacije o bolezni COVID-19 in njenem širjenju v Sloveniji, a kljub temu **ne ponujajo natančnih napovedi** za prihodnji potek epidemije, zato je **potrebno skrbno prebrati vse predpostavke modela**.

Svetovna znanost vlaga velike napore v boj z boleznijo COVID-19, a vendar mnogi vidiki širjenja in razvoja bolezni še niso raziskani. Še posebno velika je negotovost glede učinka ukrepov, ki jih vlade po svetu izvajajo za omejitev bolezni. Poleg tega je, zaradi časovnih zamikov med okužbo in potrditvijo, praktično nemogoče točno oceniti dejansko stanje okuženosti in hitrost širjenja okužbe v populaciji. Vse to so razlogi, da so modelom pripadajoči intervali nedoločenosti sorazmerno veliki, in, dlje kot gledamo v prihodnost, hitreje rastejo.

## Ensemble model
V sodelovanju z [dr. Žigo Zaplotnikom](https://twitter.com/ZaplotnikZiga) iz FMF smo pripravili "ansambel" različnih napovedi, ki upoštevajo obstoj potencialnih nosilcev, ki imajo stik z veliko ljudmi.

Tule je ilustracija:
<img src="https://fiz.fmf.uni-lj.si/~zaplotnikz/2020_03_28/plot/potek_pandemije12_share.png" width="600">

Na voljo je tudi [izvorna koda](https://github.com/zaplotnik/korona/tree/master/code) in podroben [opis modela](http://stat.columbia.edu/~jakulin/Covid/3.28.zaplotnik.model.pdf).


## SEIR model
V sodelovanju s [prof. Janezom Žibertom](https://pacs.zf.uni-lj.si/janez-zibert/) iz Zdravstvene fakultete smo pripravili model SEIR, ki ima parametre usklajene s podatki o hospitalizacijah ter klinično sliko COVID-19. Nakazuje na to, da imamo večje število okuženih, ki se jim ne sledi, zato se okužba v populaciji širi naprej.

<img src="https://covid-19.sledilnik.org/images/20200403-model-jzibert.png" width="600">

Model lahko tudi sami v [živo uporabljate](https://pacs.zf.uni-lj.si/statistika/simulacija-sirjenja-covid-19/), tako da spreminjate posamezne parametre in vidite vpliv na razvoj epidemije. Klinične in empirične predpostavke so pod podpisom.



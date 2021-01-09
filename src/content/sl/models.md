# Modeli in napovedi

V naši skupnosti sodelujejo tudi strokovnjaki za statistično modeliranje in računalniške simulacije.
Na tej strani objavljamo povezave na nekatere od njihovih modelov,
ki so jih pripravili in umerili z uporabo podatkov [COVID-19 Sledilnik](https://covid-19.sledilnik.org).
- [SEIR model](#seir)
- [Ocenjevanje stopnje reprodukcije](#reproduction-rate)
- [Model prenosa virusa po socialnem omrežju prebivalcev Slovenije](#social_network)
- [Drugi modeli za Slovenijo](#other)
- [Zanimivi prispevki na temo modeliranja epidemije](#articles)

Za osnovno razumevanje modelov o epidemiji priporočamo članek [Epidemija in modeli... - Osnove epidemiološkega modeliranja](https://medium.com/sledilnik/epidemija-in-modeli-786e02f1bd8a).

Modeli v veliki meri upoštevajo doslej znane informacije o bolezni COVID-19 in njenem širjenju v Sloveniji, a kljub temu **ne ponujajo natančnih napovedi** za prihodnji potek epidemije, **zato je potrebno skrbno prebrati vse predpostavke modela**.
Podatki o testih in potrjenih okuženih osebah v Sloveniji so skopi, zato sta oba predstavljena modela umerjena na podatke o hospitalizacijah. Pomanjkanje natančnih podatkov o primerih je eden glavnih razlogov za nedoločenosti. Zaradi sprememb v metodologiji testiranja pa se lahko predpostavke tudi popolnoma spremenijo - več lahko preberete v članku [Kako vemo, da ne vemo?](https://medium.com/sledilnik/kako-vemo-da-ne-vemo-6570b92a8b3c)

Svetovna znanost vlaga velike napore v boj z boleznijo COVID-19, a vendar mnogi vidiki širjenja in razvoja bolezni še niso raziskani. Še posebno velika je negotovost glede učinka ukrepov, ki jih vlade po svetu izvajajo za omejitev bolezni. Poleg tega je, zaradi časovnih zamikov med okužbo in potrditvijo, praktično nemogoče točno oceniti dejansko stanje okuženosti in hitrost širjenja okužbe v populaciji. Vse to so razlogi, da so modelom pripadajoči intervali nedoločenosti sorazmerno veliki, in, dlje kot gledamo v prihodnost, hitreje rastejo.

## <a id="seir"></a>SEIR model
V sodelovanju s [prof. Janezom Žibertom](https://pacs.zf.uni-lj.si/janez-zibert/) z Zdravstvene fakultete, Univerze v Ljubljani smo pripravili model SEIR (Susceptible, Exposed, Infected, and Recovered) s podmodeli za modeliranje bolnišničnih obravnav, obravnav na intenzivni negi in smrti, ki ima parametre usklajene s podatki o hospitalizacijah in klinično sliko COVID-19 v Sloveniji. 

Vsakodnevne projekcije modela se izračunavajo ob 13.00 in 17.00. Bolj podroben prikaz projekcij je na [naslednji povezavi](https://apps.lusy.fri.uni-lj.si/appsR/CoronaV2/).

Dodatne projekcije so narejene še [na spletni strani](https://apps.lusy.fri.uni-lj.si/appsR/CoronaSimV2/), kjer se izvajajo simulacije na deterministični in stohastični verziji SEIR modela in se rezultati združujejo skupaj. 

Pri interpretaciji rezultatov je potrebno upoštevati omejitve takšnih modelov. 

<a href="https://apps.lusy.fri.uni-lj.si/~janezz/zadnja-simulacija_V2.png" class="img-link">
<img alt="SEIR model" src="https://apps.lusy.fri.uni-lj.si/~janezz/zadnja-simulacija_V2.png"></a>

## <a id="reproduction-rate"></a>Ocenjevanje stopnje reprodukcije
Skupina [Inštituta za biostatistiko in medicinsko informatiko](http://ibmi.mf.uni-lj.si/) Medicinske fakultete Univerze v Ljubljani je pod vodstvom prof. Maje Pohar Perme na podlagi opaženih podatkov [ocenila stopnjo reprodukcije](http://ibmi.mf.uni-lj.si/files/Pregledni%20povzetek_74e.pdf), to je hitrost širjenja okužbe, med posamičnimi intervencijami. Pri modeliranju so uporabili tehnike Bayesovske statistike, ki omogoča ocenjevanje kompleksnih parametrov pri omejenem številu podatkov, kar nudi možnost hitrejšega odziva.

Podrobnejši opis prvotne metodologije in izsledkov je objavljen kot članek v [Zdravniškem vestniku](https://vestnik.szd.si/index.php/ZdravVest/article/view/3068).

Prvotni model je bil kasneje nadgrajen, dnevni rezultati pa se sedaj objavljajo na [tej povezavi](https://oblak8.mf.uni-lj.si/covid19/).

<a href="https://oblak8.mf.uni-lj.si/covid19/" class="img-link">
<img alt="R_t model" src="https://oblak8.mf.uni-lj.si/covid19/rt_graph.svg" width=600>
<!--<img alt="R_t model" src="/docs/ibmi-model-20200627.png">-->
<!--<img alt="R_t model" src="https://stat.columbia.edu/~jakulin/Covid/ocene_rt.png">-->
</a>

## <a id="social_network"></a>Model prenosa virusa po socialnem omrežju prebivalcev Slovenije
V sodelovanju z [dr. Žigo Zaplotnikom](https://twitter.com/ZaplotnikZiga) s Fakultete za matematiko in fiziko Univerze v Ljubljani smo pripravili verjetnostno napoved poteka pandemije v Sloveniji. V simulaciji se virus prenaša po realističnem modelu socialnega omrežja Slovencev, ki vsebuje več kot 2 milijona vozlišč (1 za vsakega prebivalca Slovenije), razdeljenih v gospodinjstva in domove oskrbovancev. Vozlišča naključno povežemo tudi izven teh enot, glede na znane porazdelitve kontaktov – nekatere  osebe imajo dnevno več kontaktov, druge manj. To omogoča, da lahko z modelom efektivno simuliramo različne strategije zajezitve virusa.Verjetnostno napoved dobimo tako, da pripravimo množico simulacij z rahlo spremenjenim začetnim pogojem in parametri, ki določajo širjenje koronavirusa in potek bolezni COVID-19. Ta se tudi med posameznimi osebami razlikuje. Podrobnejši opis modela se nahaja [v članku](https://journals.plos.org/plosone/article?id=10.1371/journal.pone.0238090), zgodovina izračunov pa [tule](https://fiz.fmf.uni-lj.si/~zaplotnikz/korona/).

<!--
<a href="https://fiz.fmf.uni-lj.si/~zaplotnikz/korona/last_forecast/potek_pandemije.png" class="img-link">
<img alt="Omrežje model" src="https://fiz.fmf.uni-lj.si/~zaplotnikz/korona/last_forecast/potek_pandemije.png"></a>
-->

## <a id="other"></a>Drugi modeli za Slovenijo

- Model v članku [Določanje alarmov pri epidemiji COVID-19 v Sloveniji (IBMI MF UL)](https://ibmi.mf.uni-lj.si/sl/centri/biostatisticni-center/interaktivno/dolocanje-alarmov-pri-epidemiji-covid-19-v-sloveniji) ocenjuje, v kolikšni meri je že prišlo do ponovnega zagona epidemije. Za oceno uporablja delež potrjeno okuženih izmed testiranih in se dnevno posodablja s podatki Sledilnika.

- [Dr. Matjaž Leskovar](http://r4.ijs.si/leskovar) z Odseka za reaktorsko tehniko Instituta Jožef Stefan pripravlja napoved razvoja epidemije v Sloveniji glede na javno dostopne podatke o potrjenih okužbah in trenutni zasedenosti bolnišnic. Na voljo je [opis modela, s povezavo na izvorno datoteko](http://r4.ijs.si/COVID19model), [arhiv napovedi](http://r4.ijs.si/COVID19arhiv) in [prikaz zadnjih rezultatov](http://r4.ijs.si/COVID19).

- [Dr. Andrej Srakar](https://sites.google.com/site/andrejsrakar1975/) je za blog Udomačena statistika napisal izvrsten pregledni članek [**Uvod v modeliranje in statistične vidike COVID-19**](https://udomacenastatistika.wordpress.com/2020/04/20/uvod-v-modeliranje-in-statisticne-vidike-covid-19/) o ključnih modelih širjenja epidemije bolezni COVID-19, ki so nastali v Sloveniji.

## <a id="articles"></a>Zanimivi prispevki na temo modeliranja epidemije

- [Kaj ima matematika z epidemijo?](https://medium.com/sledilnik/kaj-ima-matematika-z-epidemijo-155023c10221): članek, ki razloži osnovne pojme in predstavi matematični vidik modeliranja. Napisano na podlagi predavanja prof.Janeza Žiberta.

- *Predstavitev modeliranja epidemije v Državnem zboru RS*: 12. novembra 2020 so sodelavci Sledilnika predstavili pomen podatkov in modeliranja v Državnem zboru RS. Na voljo so tudi [povzetek in posnetki nastopov](https://medium.com/sledilnik/povzetek-nastopov-strokovnjakov-s-seje-parlamentarnega-odbora-12-11-2020-5a3ead7b4898).

- *Matematično modeliranje s prikazom možnih izidov epidemije pomaga oblikovati javnozdravstvene ukrepe. Da bi bili rezultati modeliranja zanesljivejši, je zelo pomembno kritično ovrednotiti uporabljene podatke ter preveriti, ali so bili upoštevani različni načini širjenja bolezni v populaciji.* – [V uvodniku revije Zdravstveno varstvo](https://content.sciendo.com/view/journals/sjph/59/3/article-p117.xml?tab_body=abstract), ki jo izdaja NIJZ, so I. Eržen, T. Kamenšek, M. Fošnarič in J. Žibert povzeli trenutna spoznanja in ključne izzive pri modeliranju epidemije COVID-19.

- *Srečanje slovenskih znanstvenikov na temo COVID-19 ukrepov*: Mlada sekcija Statističnega društva Slovenije je 21. aprila 2020 organizirala spletni pogovor, ki ga je gostil Inštitut za biostatistiko in medicinsko informatiko (IBMI) Medicinske fakultete Univerze v Ljubljani. Pogovor sta vodila dr. Andrej Srakar in dr. Ana Slavec. STA je dogodek prenašala v živo več kot 850 udeležencem. Posnetek in predavanja si lahko [preberete tukaj](https://medium.com/sledilnik/64233b35580c).

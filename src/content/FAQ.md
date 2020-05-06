<h1>FAQ - Pogosta vprašanja in odgovori</h1>

_Namen Sledilnikovih prikazov je združiti in predstaviti obstoječe podatke v vsakomur razumljivi obliki, ki pa vseeno potrebujejo določena pojasnila. Zbrali smo odgovore na nekatera najpogostejša vprašanja. Seznam sproti dopolnjujemo; če imate kakršna koli vprašanja glede naših prikazov (ali predlog za boljše pojasnilo) in jih ne najdete v FAQ, nam pišite na info@sledilnik.org. (Veseli smo tudi pohval, povratnih informacij ali predlogov!)_

## Splošno

<details>
  <summary id=why-sledilnik>Zakaj Sledilnik?</summary>

Naš cilj je pomagati pri razumevanju širjenja virusa in pripomoči k splošni ozaveščenosti, odzivnosti ter učinkovitosti ukrepov za zajezitev virusa. Več v [zavihku O projektu](/about). 

</details>

<details>
  <summary id=virus-vs-disease>Kakšna je razlika med SARS-Cov-2 in COVID-19?</summary>

**SARS-CoV-2** je angleška okrajšava za “Severe Acute Respiratory Syndrome Coronavirus 2” – to je mednarodno sprejeto ime virusa, ki povzroča bolezen **COVID-19**. Tudi slednje poimenovanje je kratica, skovana iz besed COrona VIrus Disease ter 2019, torej leta, ko je bolezen prvič izbruhnila.

</details>

<details>
  <summary id=confirmed-cases>Kakšna je razlika med novookužen in potrjeno okužen?</summary>

V Sledilniku uporabljamo terminologijo, ki je razložena v odgovoru Kaj pomeni izraz …? Za druge izraze, kot so novookužen, ki se pojavljajo v medijih, ne pa tudi v naših grafih, lahko pri uporabi pomaga [slovar Fran, različica covid-19](https://fran.si/o-portalu?page=Covid_19_2020). 

</details>

<details>
  <summary id=all-infected>Je kjer koli mogoče dobiti statistiko vseh obolelih, tudi asimptomatskih primerov?</summary>

To trenutno ni mogoče. Razlogov je več: testiranja zajemajo le določen del populacije (bolniki z znaki in simptomi akutne okužbe dihal, ki bi lahko potrebovali zdravljenje v bolnišnici, zdravstveni delavci in oskrbovanci DSO s simptomi okužbe dihal, starejši nad 60 let po presoji zdravnika), zato je v statistiko lahko zajet le del populacije, ki očitno kaže znake okužbe, mlajša oz. netestirana populacija je torej za zdaj disproporcionalno zastopana. Statistike asimptomatskih bolnikov, ki ne kažejo simptomov in niso zato nikjer zavedeni, tako ni mogoče dobiti. 

</details>

<details>
  <summary id=other-countries>Kje lahko najdem primerjavo med Slovenijo in drugimi državami?</summary>

Sledilnik trenutno ne prikazuje nobenih vizualizacij, ki bi stanje v Sloveniji primerjale s podobnimi stanji v tujini. Za takšne primerjave si lahko vedno ogledate katero od strani, kot sta [Coronavirus Pandemic](https://ourworldindata.org/coronavirus), stran raziskovalne skupine Our World in Data univerze v Oxfordu, ki omogoča prikaz in primerjavo podatkov za izbrane države, ter [COVID-19 Dashboard](https://coronavirus.jhu.edu/map.html), stran univerze Johns Hopkins (ene vodilnih raziskovalnih institucij v ZDA). Zaradi nezanesljivih podatkov je manj priporočljiva stran [Worldometer](https://www.worldometers.info/coronavirus/). Več priporočenih povezav si oglejte na [strani Povezave](/links).

</details>

<details>
  <summary id=english-translation>Ali obstaja vaša stran tudi v angleščini?</summary>

Deloma. V celoti zaenkrat ne, sta pa na voljo za prosto uporabo tako besedilni del kot izvorna koda, če bi se želel kdo lotiti tega podviga. Vsi podatki so v bazi že zavedeni tudi z angleškimi oznakami, zato je mogoča tudi njihova mednarodna uporaba (izvoz).
V angleškem jeziku obstaja samo [stran O projektu](/about/en), ki zajema osnovne podatke in vire podatkov.

</details>

<details>
  <summary id=are-you-paid>Kdo vas plačuje za izdelavo teh preglednic oz. ali s temi podatki kaj zaslužite?</summary>

Nihče oz. ne. Sledilnik je neprofitna pobuda, ustvarjena kot podpora sprotnemu zbiranju in urejanju ključnih podatkov o širjenju koronavirusa pri nas. Naša baza podatkov je javna in prosto dostopna, torej brezplačna in nekomercialna, in bo takšna tudi ostala. Gl. tudi vprašanje Kako lahko pridobim in uporabim vašo bazo podatkov.

</details>

<details>
  <summary id=tech-used>Katere tehnologije ste uporabili za izdelavo spletne strani/aplikacije?</summary>

Stran je v JavaScriptu s pomočjo Vue.js, vizualizacije in grafi so narejeni v F# s pomočjo knjižnic Highcharts, projekt pa je odprt in na voljo na [GitHubu - Sledilnik](https://github.com/sledilnik).

</details>

## O podatkih

<details>
  <summary id=data-reliability>So vaši podatki in prikazi verodostojni?</summary>

Podatke zbiramo iz različnih uradnih in drugih javnih virov – navedeni so v [zavihku Viri](/sources). 

Od 28. marca 2020 imamo vzpostavljeno tudi povezavo z Ministrstvom za zdravje, NIJZ in zdravstvenimi zavodi, od katerih zdaj neposredno dobivamo strukturirane podatke. Ekipa Sledilnika ne nadzoruje točnosti izvirnih podatkov in ne objavlja podatkov, ki niso pridobljeni iz uradnih virov ali sredstev javnega obveščanja, zato pa vse podatke navzkrižno preverja, da so pravilni in skladni z izvornimi.

</details>

<details>
  <summary id=data-collection>Kako zbirate in urejate podatke?</summary>

[Bazo podatkov](https://docs.google.com/spreadsheets/d/1N1qLMoWyi3WFGhIpPFzKsFmVE0IwNP3elb_c18t2DwY/edit#gid=0) urejamo s podatki NIJZ (po kategorijah). Podatke po regijah in starosti kdaj tudi kasneje dopolnjujemo in navzkrižno preverjamo, ko se spremenijo zaradi epidemioloških raziskav. Podatke o občinah sledimo v [tabeli Kraji](https://docs.google.com/spreadsheets/d/1N1qLMoWyi3WFGhIpPFzKsFmVE0IwNP3elb_c18t2DwY/edit#gid=598557107).

Urejanje podatkov bolnišnične oskrbe – [tabela Pacienti](https://docs.google.com/spreadsheets/d/1N1qLMoWyi3WFGhIpPFzKsFmVE0IwNP3elb_c18t2DwY/edit#gid=918589010):

- Dobivamo dnevna poročila in spremljamo objave vseh bolnišnic za COVID-19 (UKC Ljubljana, UKC Maribor, UK Golnik, SB Celje) – okoli 8h.

- Spremljamo število hospitaliziranih: vsi oddelki, v intenzivni enoti in na respiratorju.

- Iz podatkov evidentiramo tudi prehode (sprejem/odpust) med posameznimi stanji (kadar je to mogoče zaznati).

- Kjer so podatki o prehodih (sprejem/odpust) nepopolni, s sklepanjem določimo vrednosti (uporabimo formulo).

- Vsi viri in sklepanja so zabeleženi kot komentar v posameznih celicah (možnost preverjanja).

- Podatke primerjamo s sumarnimi podatki o hospitaliziranih in intenzivni negi, ki jih objavlja Vlada RS.
  
  </details>

<details>
  <summary id=data-publish-time>Kdaj objavljate podatke? Zakaj so nekateri podatki osveženi z današnjim dnem, drugi pa imajo včerajšnji datum?</summary>

Večina podatkov se zbira za pretekli dan ob 23.59 (testi, potrjene okužbe ...), podatke o hospitalizacijah pa večinoma pridobimo do 9. ure vsak dan za vse bolnišnice. **Naši podatki so tako osveženi ponavadi med 10.00 in 12.00**.  

Ko objavimo sveže dnevne podatke, so ti na voljo na vseh naših distribucijskih poteh (CSV, REST, spletna stran), o objavi poročamo tudi na družbenih omrežjih ([Facebook](https://www.facebook.com/COVID19Sledilnik) in [Twitter](https://twitter.com/sledilnik)).

</details>

<details>
  <summary id=data-differences>Na drugih portalih so številke obolelih drugačne kot pri vas. Zakaj?</summary>

Sledilnik uporablja zgolj potrjene, uradne podatke, ki jih dnevno sporočajo NIJZ in vse slovenske bolnišnice, ki zdravijo bolezen COVID-19. Naši podatki tako prihajajo neposredno iz preverjenih virov, hkrati pa jih tudi sami navzkrižno primerjamo že od začetka delovanja (4. 3. 2020). Razlike v objavljenih podatkih se po navadi pojavijo zato, ker so bili zajeti ob različnih urah dneva.

(Gl. tudi vprašanje <a href="#data-reliability">So podatki verodostojni?</a>)</p>

</details>

<details>
  <summary id=data-hospital-in>Zakaj poleg trenutno hospitaliziranih ne prikazujete tudi, koliko ljudi je bilo na določen dan sprejetih v hospitalizacijo?</summary>

Bolnišnice o posameznih sprejemih ali odpustih, iz katerih bi lahko pridobili natančne podatke, ne poročajo. Število sprejemov in odpustov na določen dan lahko zaznamo le iz spremembe trenutno hospitaliziranih. Če je bilo recimo pet sprejetih in štirje odpuščeni, je iz naših podatkov zaznan le en sprejem. Ker želimo na grafih sporočati le dejanske, preverjene podatke, pomanjkljivih podatkov o številu hospitaliziranih na dan ne bomo vključili, dokler se sistematika sledenja ne spremeni. 

</details>

<details>
  <summary id=data-hospital-out>Kako pridobivate podatke o odpuščenih iz bolnišnic?</summary>

Podatek **Odpuščeni iz bolnišnice** je Sledilnikova ocena, izračunana na podlagi podatkov, ki jih dnevno dobivamo neposredno iz bolnišnic, torej iz preverjenega vira. Ker pa bolnišnice ne poročajo vseh sprejemov in odpustov iz bolnišnice, je naša ocena narejena na podlagi spremembe trenutno hospitaliziranih pacientov (če trenutno število pacientov pade, sklepamo, da so bili odpuščeni). Zato je naša ocena konzervativna (nižja od dejanskega števila odpuščenih bolnikov).

</details>

<details>
  <summary id=data-recovered>Zakaj tako dolgo niste prikazovali števila ozdravelih in zakaj zdaj namesto teh prikazujete prebolele?</summary>

Sledilnik se je pri številu ozdravelih zanašal na uradne vire (Vlada RS, mediji). Poročanja o ozdravelih so žal še vedno redka – za zdaj imamo samo par potrjenih virov o "ozdravelih", rednih podatkov in uradnih virov pa ni, kakor tudi ne uradne defincije, kdaj je določena oseba ozdravela. V okviru Inštituta za mikrobiologijo in imunologijo trenutno poteka [nacionalna raziskava o COVID-19](https://covid19.biolab.si/), ki bo s prostovoljnim testiranjem na vzorcu pokazala tudi, koliko ljudi je bolezen COVID-19 že prebolelo. Ker trenutno še ni znano, kakšne so morebitne posledice prebolele bolezni COVID-19 (s tem se ukvarjajo različne študije, rezultati pa še dolgo ne bodo znani), in ker tudi zdravstvene institucije govorijo o preboleli bolezni (in ne o ozdravelih), smo skladno s tem spremenili tako terminologijo kot način izračunavanja števila prebolelih. (Gl. tudi vprašanje Ali vodite števec aktivnih okužb oz. ali veste, koliko je trenutno okuženih oseb?) 

V vednost: Ministrstvo za zdravje je 14. aprila objavilo [Priporočila za zaključek izolacije in vrnitev na delovno mesto](https://www.zbornica-zveza.si/wp-content/uploads/2020/04/PRIPORO%C4%8CILO-Zaklju%C4%8Dek-izolacije-in-vrnitev-na-delovna-mesta-po-preboleli-bolezni-COVID-19.pdf), iz katerih lahko razberemo, kdaj se za osebo sklepa, da je prebolela okužbo in se lahko vrne na delo. Za osebe s simptomi je to 14 dni po umiritvi simptomov, za zdravstvene delavce je po 14 dneh obvezen kontrolni bris, ki mora biti negativen 2x zapored. Vlada RS sicer redno poroča o odpuščenih iz bolnišnice, za katere pa ne vemo, ali so res že preboleli bolezen. Iz objavljenih priporočil je razvidno, da sta pri teh bolnikih po odpustu v domačo oskrbo potrebna dva zaporedna negativna kontrolna brisa, da bi se oseba štela za sposobno vrnitve na delo.  

Opazili smo, da [Worldometer](https://www.worldometers.info/coronavirus/#countries) poroča o številu okrevanj, a žal nam podatka, od kod črpajo te informacije, ni uspelo pridobiti. Tudi nekateri drugi viri preprosto združujejo prebolele osebe s številom odpuščenih bolnikov iz bolnišnic. Ker menimo, da ta dva podatka ne kažeta enakega stanja bolezni, smo se odločili, da jih prikazujemo ločeno (kazalnika Odpuščeni in Preboleli). 

</details>

<details>
  <summary id=data-active-cases>Ali vodite števec aktivnih okužb oz. ali veste, koliko je trenutno okuženih oseb?</summary>

Da, od konca aprila naprej grafično prikazujemo tudi te kazalnike – **Potrjeno okuženi (aktivni)** in **Preboleli (skupaj)**. 

Pri teh prikazih ne gre za podatke iz javnih virov; oba kazalnika kažeta na osnovi uradnih podatkov izračunano vrednost, zato sta za lažje razločevanje prikazana s črtkano črto. Vrednost *Potrjeno okuženi (aktivni)* je izračunana s preprostim odštevanjem uradnih podatkov za relevantno kategorijo, vrednost *Preboleli (skupaj)* odslikava stanje vseh potrjeno okuženih pred tremi tedni (minus umrli). Število prebolelih je preprosta ocena, ki temelji na vrednosti vseh potrjeno okuženih v preteklosti na podlagi domneve, da se bolezen povprečno preboli najkasneje v 21 dneh; tako je število prebolelih na določen dan enako številu vseh potrjeno okuženih tri tedne pred danim datumom, od katerega se odšteje še število umrlih do istega dne, ko se ugotavlja število prebolelih. Ocena je poenostavljena v smislu, da ne upošteva primerov resnejših dolgotrajnih komplikacij bolezni COVID-19. 

Formula za izračun vrednosti:
- Preboleli (skupaj) = Potrjeno okuženi (skupaj) pred 21 dnevi – Umrli (skupaj) do dneva izračuna

- Potrjeno okuženi (aktivni) = Potrjeno okuženi (skupaj) − Preboleli (skupaj) − Umrli (skupaj)

</details>

<details>
  <summary id=data-contribute>Kako se lahko uporabniki aktivno vključimo v oddajo podatkov? Kako lahko sodelujem?</summary>

Sledilnik ne zbira osebnih podatkov uporabnikov niti podatkov, ki bi jih želeli o svojem stanju ali o stanju v bolnišnicah posredovati posamezniki.

Lahko pa uporabniki prostovoljno pomagate z zbiranjem in preverjanjem podatkov iz medijev (in tudi s terena), pri statističnih in drugih analizah ipd. Za takšno obliko sodelovanja, opozorila in konstruktivne predloge nam pišite na info@sledilnik.org.

</details>

<details>
  <summary id=data-collection>Kako urejate podatke?</summary>

Celoten postopek zbiranja in urejanja podatkov je opisan na strani [O projektu](/about).

</details>

<details>
  <summary id=data-usage>Kako lahko pridobim in uporabim vašo bazo podatkov?</summary>

Naša baza podatkov je javna in prosto dostopna v obliki [**CSV**, **REST** in **Google Sheet**](/datasources). Prosimo vas le, da nam sporočite, s kakšnim namenom boste podatke uporabili, ter Sledilnik obvezno navedete kot vir.

Ker so oznake podatkov tudi v angleščini (gl. vprašanje Ali obstaja vaša stran tudi v angleščini?), je mogoča tudi njihova mednarodna uporaba (izvoz, prikaz).

</details>

## O izračunih in grafih

<details>
  <summary id=chart-usage>Lahko uporabim vaše grafe na svoji strani? Kako?</summary>

Lahko! Na svojo spletno stran lahko vgradite poljuben graf ali prikaz – ob navedbi vira, seveda. [Kliknite sem](/embed) in s seznama izberite graf, ki ga želite vgraditi. O uporabi nas obvestite (info@sledilnik.org) in povezavo bomo z veseljem dodali tudi v našo zbirko [priporočenih povezav](/links). 

</details>

<details>
  <summary id=chart-infocard-percent>Kaj pomenijo odstotki, ki se pojavljajo v informativnih okvirčkih na vrhu strani?</summary>

Gre za odstotno stopnjo rasti na današnji dan v številu oseb glede na prejšnji dan. Če je, recimo, včeraj bilo v intenzivni enoti 16 oseb, danes pa so sprejeli še štiri, je to 25 % več glede na včerajšnje stanje.  

</details>

<details>
  <summary id=chart-metrics>Kaj kaže graf "Širjenje COVID-19 v Sloveniji"?</summary>

Graf prikazuje dnevno in skupno dinamiko širjenja okužbe od začetka do danes. Uporabljeni kazalniki (gl. Katere kazalnike vključuje graf o širjenju?) nam pomagajo razumeti, ali in kako uspešno obvladujemo širjenje virusa. Spremljamo lahko, kakšen je dnevni prirast okuženih, in posredno vidimo, ali ukrepi delujejo; iz podatka o številu hospitaliziranih in deleža teh v intenzivni enoti lahko razberemo, koliko oseb je bolezen resno ogrozila, hkrati pa nam ti podatki kažejo tudi, kolikšna je obremenjenost zdravstvenega sistema.

Spodaj na časovnem traku so označene prelomne točke: od prvega potrjenega primera (4. 3. 2020) do ukrepov, sprejetih za zajezitev širjenja, kot so si sledili: Slovenija uvede vstopne točke za testiranje, zapre mejo z Italijo …, kar nam pomaga spremljati dinamiko spremenljivk glede na ukrepe.  

</details>

<details>
  <summary id=chart-metrics-included>Katere kazalnike vključuje graf "Širjenje COVID-19 v Sloveniji"?</summary>

* **Testiranja (na dan)** = Število opravljenih testiranj na prisotnost virusa SARS-CoV-2, ki povzroča bolezen COVID-19. V prvih fazah epidemije je to bil pomemben pokazatelj razširjenosti virusa, a se je s spremembo metodologije testiranja oz. vzorca testiranih to spremenilo v kazalec kapacitete zdravstvenega oz. diagnostičnega sistema.

* **Testiranja (skupaj)** = Vsota testiranj do dne; podatek je uporaben v smislu primerjave oz. deleža celotne populacije, vendar je zavajajoč, saj so določene osebe lahko testirane večkrat (npr. zdravstveni delavci, zaposleni v DSO ipd.).

* **Potrjeno okuženi (na dan)** = Število potrjeno okuženih na dan na podlagi testov. Ta kazalec ne odraža dejanskega gibanja novih okuženih v populaciji, saj se s testi ne vzorči celotne populacije, ampak se ciljno testira rizične in poklicne skupine.

* **Potrjeno okuženi (skupaj)** = Skupno število vseh potrjeno okuženih oseb do določenega dne.

* **Potrjeno okuženi (aktivni)** = Potrjeno okuženi (skupaj) – Preboleli (skupaj) – Umrli (skupaj)

* **Preboleli (skupaj)** = Število prebolelih na določen dan je preprosta ocena, enaka številu vseh potrjeno okuženih tri tedne pred danim datumom (ob predpostavki, da se bolezen povprečno preboli najkasneje v 21 dneh), od katerega se odšteje še število umrlih do istega dne, ko se ugotavlja število prebolelih.

* **Hospitalizirani (aktivni)** = Trenutno število oseb v bolnišnični oskrbi (na navadnem oddelku ali v enoti za intenzivno terapijo).

* **Hospitalizirani (skupaj)** = Vsota sprejetih v bolnišnico do dne.

* **V intenzivni enoti (aktivni)** = Trenutno število oseb v enotah intenzivne terapije.

* **Na respiratorju (aktivni)** = Trenutno število oseb, ki za dihanje potrebujejo respirator (medicinski ventilator).

* **Odpuščeni iz bolnišnice (na dan)** = Število odpuščenih iz bolnišnice na ta dan.

* **Odpuščeni iz bolnišnice (skupaj)** = Vsota vseh odpuščenih iz bolnišnice do tega dne.

* **Umrli (na dan)** = Število umrlih za posledicami COVID-19 na ta dan.

* **Umrli (skupaj)** = Vsota vseh umrlih do tega dne.
  
  </details>

<details>
  <summary id=chart-terminology>Kaj pomeni izraz …? </summary>

* **potrjeno okuženi** = To je število oseb, ki so bile pozitivne na testu prisotnosti virusa SARS-CoV-2. Ker je število potrjeno okuženih oseb odvisno zgolj od testiranja in ker zaradi spremenjene politike testiranja večina okuženih z blagimi simptomi sploh ne bo testirana na prisotnost COVID-19, je podatek o potrjeno okuženih bistveno manjši od dejanskega števila okuženih ljudi.

* **hospitalizirani** = To je število okuženih oseb, ki imajo tako resne simptome bolezni COVID-19, da so bile sprejete v bolnišnično oskrbo. 

* **v intenzivni enoti** = Označuje število hospitaliziranih oseb, ki so zaradi simptomov bolezni COVID-19 v življenjski nevarnosti in potrebujejo namestitev v enoti za intenzivno terapijo. Gre za podmnožico kategorije *Hospitalizirani*. 

* **na respiratorju** = Označuje število hospitaliziranih oseb v intenzivni enoti, ki za dihanje potrebujejo respirator (medicinski ventilator). Gre za podmnožico kategorije *V intenzivni negi* in kategorije *Hospitalizirani*.

* **preboleli** = To je ocena števila oseb, ki so bile potrjeno okužene in naj bi po 21 dneh prebolele bolezen. Število prebolelih je tako enako številu vseh potrjeno okuženih tri tedne pred danim datumom – skladno z domnevo, da bi morali najkasneje v 21 dneh bolezen preboleti –, od katerega se odšteje še število umrlih do istega dne, ko se ugotavlja število prebolelih. (Gl. tudi vprašanje Zakaj tako dolgo niste prikazovali števila ozdravelih in zakaj zdaj namesto teh prikazujete prebolele?)
  
  </details>

<details>
  <summary id=chart-phases>Kaj pomenijo različne faze (faze 1–faza 6), ki jih vidimo v grafih?</summary>

Navpične črte delijo faze, zamejene z datumi, ko so odgovorni organi spremenili način zbiranja informacij o širjenju okužbe (spremeni se način testiranja, uvedejo se interventni ukrepi samoizolacije, prepovedi zbiranja in gibanja oseb ter obvezne nošnje osnovne zaščite).

Faze so prikazane zato, ker se je s spremembo metodologije testiranja spremenil tudi pomen določenih kazalcev, po katerih lahko presojamo razširjenost okužb.

* **Faza 1 (4.–12. marec 2020)**: Zabeleženi so prvi primeri okužbe pri nas. Sledi se vsem primerom, testirajo se vsi kontakti. 

* **Faza 2 (13.–19. marec 2020)**: Spremeni se [metodologija testiranja](https://www.gov.si/novice/2020-03-14-spremenjeno-diagnosticiranje-za-realnejse-nacrtovanje-ukrepov-za-obvladovanje-epidemije/), uvedejo se interventni ukrepi o samoizolaciji in socialnem distanciranju.

* **Faza 3 (20. marec–7. april)**: Ponovno [se spremeni metodologija testiranja](https://www.gov.si/novice/2020-03-22-ministrstvo-za-zdravje-z-vrsto-ukrepov-v-boju-proti-covid-19/), vzpostavi se prepoved zbiranja več kot petih oseb na javnih površinah.

* **Faza 4 (8.–15. april)**: Nova [sprememba metodologije testiranja](https://www.gov.si/assets/ministrstva/MZ/DOKUMENTI/Koronavirus/145-Dopolnitev-navodil-glede-testiranja-na-COVID-19.pdf) – dodatno se testirajo tudi osebe z blagimi simptomi iz gospodinjstev, v katerih je več oseb z okužbo dihal.

* **Faza 5 (15.–21. april)**: Nova [sprememba metodologije testiranja](https://www.gov.si/assets/ministrstva/MZ/DOKUMENTI/Koronavirus/Druga-dopolnitev-navodil-za-testiranje-na-COVID-19.pdf) – ponovno se **po možnosti** testirajo **vse** osebe, pri katerih obstaja sum za mogočo okužbo s SARS-CoV-2 virusom.

* **Faza 6 (21. april–danes)**: Nova [sprememba metodologije testiranja](https://www.gov.si/assets/ministrstva/MZ/DOKUMENTI/Koronavirus/Dodatno-k-Drugi-dopolnitvi-navodil-za-testiranje-na-COVID19-Testiranje-pri-vseh-osebah-s-sumom.pdf) – ponovno se testirajo **vse** osebe, pri katerih obstaja sum za mogočo okužbo s SARS-CoV-2 virusom. Začne se [nacionalna raziskava](https://www.gov.si/novice/slovenija-bo-kot-prva-drzava-izvedla-raziskavo-koliko-ljudi-je-bolezen-covid19-nevede-prebolelo/) 3000 naključnih oseb (dodatna testiranja, testiranje krvi na prisotnost protiteles).

</details>

<details>
  <summary id=chart-patients>Kaj nam pove graf "Obravnava hospitaliziranih"?</summary>

Graf ima dva prikaza, eden nam kaže število oseb v bolnišnični oskrbi na ta dan po bolnišnicah, če pa pogled spremenimo s klikom na Obravnava po pacientih, vidimo celotno sliko hospitalizacij glede na stanje pacientov: kolikšno število hospitaliziranih je v enoti intenzivne nege, koliko od teh je v kritičnem stanju in potrebuje respirator, koliko je odpuščenih in umrlih. 

To je lahko osnova za presojo bolnišničnih zmogljivosti in načrtovanje morebitnega povečanja zmogljivosti. Po besedah ministra za zdravje Tomaža Gantarja: "Za bolnike s COVID-19 imamo v bolnišnicah pripravljenih 539 postelj, po potrebi se ta zmogljivost lahko poveča do 1000 postelj, ... Za intenzivno nego imamo trenutno na razpolago 56 postelj." Če vemo, da traja hospitalizacija nekoga v intenzivni enoti pri nas pribl. 14 dni ([po besedah dr. Matjaža Jereba](https://www.rtvslo.si/zdravje/novi-koronavirus/matjaz-jereb-smrtnost-kriticno-bolnih-na-oddelku-ni-velika/519962); svetovno povprečje je 3–6 tednov), lahko graf ponudi dober uvid o obremenitvi bolnišnic. 

</details>


<details>
  <summary id=chart-spread-pages>Kaj pomenijo različni prikazi dnevnega prirasta v grafu "Prirast potrjeno okuženih"?</summary>

* **Absolutni dnevni prirast** prikazuje število novih primerov potrjeno okuženih na določen dan.

V obdobju eksponentne rasti pa prikazujemo še dva dodatna pogleda:

* **Relativni dnevni prirast** prikazuje odstotno vrednost novih potrjeno okuženih na določen dan.

* **Eksponentna rast v dnevih** prikazuje faktor, v koliko dneh se število potrjeno okuženih podvoji.
  
</details>

<details>
  <summary id=chart-spread>Zakaj kaže graf "Prirast potrjeno okuženih" primerjavo z Južno Korejo?</summary>

V obdobju eksponentne rasti na grafu "Prirast potrjeno okuženih" dodatno prikazujemo pogled **Eksponentna rast v dnevih** na katerem lahko vidimo povprečje rasti v istem časovnem obdobju tudi za Južno Korejo. To smo izbrali za primerjavo zato, ker ji je kljub močnemu izbruhu bolezni COVID-19 uspelo z različnimi metodami “sploščiti krivuljo” oz. povedano drugače – Južna Koreja je ena najuspešnejših držav pri obvladovanju epidemije.

</details>

<details>
  <summary id=chart-cases>Kaj pomenijo zaprti primeri in kaj so aktivni primeri? </summary>

**Zaprti primeri** so seštevek vseh potrjeno okuženih, ki niso več okuženi z virusom, torej ozdravljenih oseb in mrtvih.

**Aktivni primeri** pomenijo vse potrjene okužbe z virusom, ki so še vedno aktualne (osebe virus še vedno prebolevajo).

</details>

<details>
  <summary id=chart-double-rate>Kako se izračunava “podvojitev v N dneh” in kaj pomeni?</summary>

V obdobju eksponentne rasti na prikazu **Potrjeno okuženi po občinah** prikazujemo oceno **Podvojitev v N dneh**, ki pomeni, da se bo število okuženih v določeni občini predvidoma podvojilo v navedenem številu dni. To je ocena povprečne hitrosti eksponentnega naraščanja, ki temelji na podatkih iz prejšnjih dni, tako da se ugotovi dan, ko se je vrednost prepolovila.

</details>

<details>
  <summary id=chart-reality>So vaši grafi slika realnega stanja?</summary>

Da, kolikor so lahko, če se zavedamo omejitev trenutnih prikazov: grafi na tej strani prikazujejo le tisto, kar je mogoče ugotoviti glede na dane podatke. Tako recimo skupno število testiranj pomeni število vseh opravljenih testov do danes, ne izraža pa skupnega števila vseh testiranih oseb, saj so nekatere osebe, na primer zdravstveni delavci in osebe, pri katerih sumijo na okužbo, testirane večkrat.

Po drugi strani je število potrjeno okuženih oseb odvisno zgolj od testiranja, in ker zaradi spremenjene politike testiranja večina okuženih z blagimi simptomi sploh ne bo testirana na prisotnost COVID-19, je podatek o potrjeno okuženih bistveno manjši od dejanskega števila okuženih ljudi.

Zato je treba te kategorije jemati z védenjem, kaj pomenijo, in interpretirati grafe z zrncem soli.

</details>

<details>
  <summary id=chart-percentage>Kako računate odstotni (%) prirast? </summary>

Za odstotni prirast vzamemo trenutno vrednost spremenljivke in od nje odštejemo stanje prejšnjega dne. Dobljeno razliko delimo s stanjem prejšnjega dne in jo pomnožimo s 100, da dobimo odstotni prirast, ki ga za potrebe predstavitve zaokrožimo na eno decimalko natančno.

Zavedamo se, da obstajajo drugačne metode, ki odstotni prirast prikazujejo drugače, vendar se nam je uporabljena metoda zdela za naše razmere in namen najprimernejša in najlažje razumljiva.

</details>

<details>
  <summary id=chart-log-scale>Kaj pomeni logaritemska skala na Y osi in kako deluje?</summary>

Logaritemska skala na navpični osi (ordinata, Y os) je izjemno uporabna za prikaz funkcij oz. količin, ki zelo hitro naraščajo – recimo za t.i. eksponentno rast okuženih –, saj bi v navadnem merilu hitro prerasla najvišjo vrednost na ordinatni osi. 

</details>

<details>
  <summary id=chart-exp-growth>Kaj pomeni “eksponentna rast okuženih”? Kako lahko merimo, s kakšno hitrostjo se širi epidemija?</summary>

Pri epidemijah nalezljivih bolezni je zelo pomembna hitrost širjenja oz. stopnja rasti okužb, saj to vpliva tudi na število obolelih in smrti. Če se število okužb v nekem določenem času povečuje za enako število, npr. za 10 vsake tri dni – 10, 20, 30, 40 ..., gre za *linearno rast primerov*; če pa se število okužb v določenem časovnem obdobju podvoji, recimo podvojitev za 10 vsake 3 dni – 10, 20, 40, 80 …, pa govorimo o *eksponentni rasti*, ki v kratkem času privede do zelo velikega števila obolelih.

Čas podvojitve kot kazalec hitrosti širjenja epidemije se spreminja (pada, raste), zato ga ne smemo preprosto projicirati v prihodnost; kaže nam zgolj trenutno hitrost podvajanja primerov na podlagi podatkov iz preteklosti.

</details>

## O projektu

<details>
  <summary id=what-is-sledilnik>Kaj je Sledilnik?</summary>

[Sledilnik je projekt](/about), ki zbira, analizira in prikazuje nekaj najbolj uporabnih podatkov, da bi lahko bolje razumeli širjenje pandemije koronavirusa in bolezni COVID-19 skupaj z njeno dinamiko in obsegom. 

Želimo si jasno predstaviti, kaj nam trenutni podatki in pregledi govorijo o širjenju virusa v Sloveniji, in zagotoviti, da postanejo informacije o obsegu in resnosti problema COVID-19 v Sloveniji vsem dostopne in čim bolj razumljive. 

</details>

<details>
  <summary id=add-link>Imam odlično povezavo na stran, ki je vi še nimate v povezavah, pa bi si tam zaslužila biti. Jo boste dodali?</summary>

Pišite nam na info@sledilnik.org – predlagano povezavo bomo preverili in jo, če je stran verodostojna in koristna, z veseljem vključili med naše povezave.

Če želite narediti še korak dlje in prispevati k skupnemu cilju, nam na [GitHubu](https://github.com/sledilnik/website/blob/master/src/content/links.md) oddajte Pull-Request (PR).</p>

</details>

<details>
  <summary id=how-to-help>Želim pomagati, kje lahko začnem?</summary>

Pišite nam na info@sledilnik.org in na kratko opišite, kdo ste in kako lahko prispevate k projektu. Vabljeni!

</details>

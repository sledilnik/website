<h1>FAQ - Pogosta vprašanja in odgovori</h1>

_Namen Sledilnikovih prikazov je združiti in predstaviti obstoječe podatke v vsakomur razumljivi obliki, ki pa vseeno potrebujejo določena pojasnila. Zbrali smo odgovore na nekatera najpogostejša vprašanja. Seznam sproti dopolnjujemo; če imate kakršna koli vprašanja glede naših prikazov (ali predlog za boljše pojasnilo) in jih ne najdete v FAQ, nam pišite na info@sledilnik.org. (Veseli smo tudi pohval, povratnih informacij ali predlogov!)_

## Splošno
<details>
  <summary>Zakaj Sledilnik?</summary>
  
Naš cilj je pomagati pri razumevanju širjenja virusa in pripomoči k splošni ozaveščenosti, odzivnosti ter učinkovitosti ukrepov za zajezitev virusa. Več v [zavihku O projektu](https://covid-19.sledilnik.org/#/about). 
</details>

<details>
  <summary>Kakšna je razlika med SARS-Cov-2 in COVID-19?</summary>

**SARS-CoV-2** je angleška okrajšava za “Severe Acute Respiratory Syndrome Coronavirus 2” – to je mednarodno sprejeto ime virusa, ki povzroča bolezen **COVID-19**. Tudi slednje poimenovanje je kratica, skovana iz besed COrona VIrus Disease ter 2019, torej leta, ko je bolezen prvič izbruhnila.
</details>

<details>
  <summary>Kakšna je razlika med novookužen in potrjeno okužen?</summary>
  
V Sledilniku uporabljamo terminologijo, ki je razložena v odgovoru Kaj pomeni izraz …? Za druge izraze, kot so novookužen, ki se pojavljajo v medijih, ne pa tudi v naših grafih, lahko pri uporabi pomaga [slovar Fran, različica covid-19](https://fran.si/o-portalu?page=Covid_19_2020). 
</details>

<details>
  <summary>Je kjer koli mogoče dobiti statistiko vseh obolelih, tudi asimptomatskih primerov?</summary>
  
To trenutno ni mogoče. Razlogov je več: testiranja zajemajo le določen del populacije (bolniki z znaki in simptomi akutne okužbe dihal, ki bi lahko potrebovali zdravljenje v bolnišnici, zdravstveni delavci in oskrbovanci DSO s simptomi okužbe dihal, starejši nad 60 let po presoji zdravnika), zato je v statistiko lahko zajet le del populacije, ki očitno kaže znake okužbe, mlajša oz. netestirana populacija je torej za zdaj disproporcionalno zastopana. Statistike asimptomatskih bolnikov, ki ne kažejo simptomov in niso zato nikjer zavedeni, tako ni mogoče dobiti. 
</details>

<details>
  <summary>Kje lahko najdem primerjavo med Slovenijo in drugimi državami?</summary>

Sledilnik trenutno ne prikazuje nobenih vizualizacij, ki bi stanje v Sloveniji primerjale s podobnimi stanji v tujini. Za takšne primerjave si lahko vedno ogledate katero od strani, kot je [Worldometer](https://www.worldometers.info/coronavirus/), oz. preverite na [strani Povezave](https://covid-19.sledilnik.org/#/links).
</details>

<details>
  <summary>Ali obstaja vaša stran tudi v angleščini?</summary>

Zaenkrat ne, sta pa na voljo za prosto uporabo tako besedilni del kot izvorna koda, če bi se želel kdo lotiti tega podviga. Vsi podatki so v bazi že zavedeni tudi z angleškimi oznakami, zato je mogoča tudi njihova mednarodna uporaba (izvoz).
</details>

<details>
  <summary>Kdo vas plačuje za izdelavo teh preglednic oz. ali s temi podatki kaj zaslužite?</summary>

Nihče oz. ne. Sledilnik je neprofitna pobuda, ustvarjena kot podpora sprotnemu zbiranju in urejanju ključnih podatkov o širjenju koronavirusa pri nas. Naša baza podatkov je javna in prosto dostopna, torej brezplačna in nekomercialna, in bo takšna tudi ostala. Gl. tudi vprašanje Kako lahko pridobim in uporabim vašo bazo podatkov.
</details>

<details>
  <summary>Katere tehnologije ste uporabili za izdelavo spletne strani/aplikacije?</summary>

Stran gostuje na [GitHub Pages]([https://pages.github.com](https://pages.github.com)). Narejena je v JavaScriptu s pomočjo Vue.js, vizualizacije in grafi so narejeni v F# s pomočjo knjižnic Highcharts in Google Charts, projekt pa je odprt in na voljo na [GitHubu - Sledilnik](https://github.com/slo-covid-19).
</details>

## O podatkih

<details>
  <summary>So vaši podatki in prikazi verodostojni?</summary>
  
Podatke zbiramo iz različnih uradnih in drugih javnih virov – navedeni so v [zavihku Viri](https://covid-19.sledilnik.org/#/sources). 

Od 28. marca 2020 imamo vzpostavljeno tudi povezavo z Ministrstvom za zdravje, NIJZ in zdravstvenimi zavodi, od katerih zdaj neposredno dobivamo strukturirane podatke. Ekipa Sledilnika ne nadzoruje točnosti izvirnih podatkov in ne objavlja podatkov, ki niso pridobljeni iz uradnih virov ali sredstev javnega obveščanja, zato pa vse podatke navzkrižno preverja, da so pravilni in skladni z izvornimi.
</details>


<details>
  <summary>Kdaj objavljate podatke? Zakaj so nekateri podatki osveženi z današnjim dnem, drugi pa imajo včerajšnji datum?</summary>

Večina podatkov se zbira za pretekli dan ob 23.59 (testi, potrjene okužbe,...), podatke o hospitalizacijah pa večinoma pridobimo do 9. ure vsak dan za vse bolnišnice. **Naši podatki so tako osveženi ponavadi med 10.00 in 12.00**.  

Ko objavimo sveže dnevne podatke, so ti na voljo na vseh naših distribucijskih poteh (CSV, REST, spletna stran), o objavi poročamo tudi na družbenih omrežjih ([Facebook](https://www.facebook.com/COVID19Sledilnik) in [Twitter](https://twitter.com/sledilnik)).
</details>


<details>
  <summary>Na drugih portalih so številke obolelih drugačne kot pri vas. Zakaj?</summary>

Sledilnik uporablja zgolj potrjene, uradne podatke, ki jih dnevno sporočajo NIJZ in vse slovenske bolnišnice, ki zdravijo bolezen COVID-19. Naši podatki tako prihajajo neposredno iz preverjenih virov, hkrati pa jih tudi sami navzkrižno primerjamo že od začetka delovanja (4. 3. 2020). Razlike v objavljenih podatkih se po navadi pojavijo zato, ker so bili zajeti ob različnih urah dneva.

(Gl. tudi vprašanje So podatki verodostojni?)</p>
</details>

<details>
  <summary>Zakaj poleg trenutno hospitaliziranih ne prikazujete tudi, koliko ljudi je bilo na določen dan sprejetih v hospitalizacijo?</summary>

Bolnišnice o posameznih sprejemih ali odpustih, iz katerih bi lahko pridobili natančne podatke, ne poročajo. Število sprejemov in odpustov na določen dan lahko zaznamo le iz spremembe trenutno hospitaliziranih. Če je bilo recimo pet sprejetih in štirje odpuščeni, je iz naših podatkov zaznan le en sprejem. Ker želimo na grafih sporočati le dejanske, preverjene podatke, pomanjkljivih podatkov o številu hospitaliziranih na dan ne bomo vključili, dokler se sistematika sledenja ne spremeni. 
</details>

<details>
  <summary>Kako pridobivate podatke o odpuščenih iz bolnišnic?</summary>

Podatek **Odpuščeni iz bolnišnice** je Sledilnikova ocena, izračunana na podlagi podatkov, ki jih dnevno dobivamo neposredno iz bolnišnic, torej iz preverjenega vira. Ker pa bolnišnice ne poročajo vseh sprejemov in odpustov iz bolnišnice, je naša ocena narejena na podlagi spremembe trenutno hospitaliziranih pacientov (če trenutno število pacientov pade, sklepamo, da so bili odpuščeni). Zato je naša ocena konzervativna (nižja od dejanskega števila odpuščenih bolnikov).
</details>

<details>
  <summary>Zakaj je prikazano število ozdravelih manjše od števila odpuščenih iz bolnišnične oskrbe?</summary>
  
Sledilnik se pri številu ozdravelih zanaša na uradne vire (Vlada RS, mediji). Poročanja o ozdravljenih so žal redka – za zdaj imamo samo par potrjenih virov o ozdravljenih. 

Ministrstvo za zdravje je 14. aprila objavilo [Priporočila za zaključek izolacije in vrnitev na delovno mesto](https://www.zbornica-zveza.si/wp-content/uploads/2020/04/PRIPORO%C4%8CILO-Zaklju%C4%8Dek-izolacije-in-vrnitev-na-delovna-mesta-po-preboleli-bolezni-COVID-19.pdf), iz katerih bi lahko sklepali, kdaj se oseba obravnava kot ozdravljena in je sposobna za vrnitev na delo: za osebe s simptomi je to 14 dni po umiritvi simptomov, za zdravstvene delavce je po 14 dneh obvezen kontrolni bris, ki mora biti negativen 2x zapored. Vlada RS sicer redno poroča o odpuščenih iz bolnišnice, za katere pa ne vemo, ali so res ozdravljeni. Iz objavljenih priporočil je razvidno, da sta pri teh bolnikih po odpustu v domačo oskrbo potrebna dva zaporedna negativna kontrolna brisa, da bi se oseba štela za sposobno vrnitve na delo.  

Opazili smo, da [Worldometer](https://www.worldometers.info/coronavirus/#countries) poroča o več ozdravelih, a žal nam podatka, od kod črpajo te informacije, ni uspelo pridobiti. Tudi nekateri drugi viri preprosto združujejo ozdravele skupaj s številom odpuščenih bolnikov iz bolnišnic. Ker menimo, da ta dva podatka ne kažeta enakega stanja bolezni, ju v Sledilniku prikazujemo ločeno.
</details>

<details>
  <summary>Ali vodite števec aktivnih okužb oz. ali veste, koliko je trenutno okuženih oseb?</summary>
  
Da, števec aktivnih okužb redno vodimo in ga lahko najdete v [Google Sheets tabeli](https://docs.google.com/spreadsheets/d/1N1qLMoWyi3WFGhIpPFzKsFmVE0IwNP3elb_c18t2DwY/edit#gid=0), vendar ga grafično ne prikazujemo, saj je podatkov o ozdravelih relativno malo in niti ni znano, ali so res vsi zavedeni. Zato graf trenutno prikazuje le skupno število potrjeno okuženih od začetka do danes, ki so bile na testu pozitivne – vključno z osebami, ki so okrevale, in umrlimi. 
</details>

<details>
  <summary>Kako se lahko uporabniki aktivno vključimo v oddajo podatkov? Kako lahko sodelujem?</summary>

Sledilnik ne zbira osebnih podatkov uporabnikov niti podatkov, ki bi jih želeli o svojem stanju ali o stanju v bolnišnicah posredovati posamezniki.

Lahko pa uporabniki prostovoljno pomagate z zbiranjem in preverjanjem podatkov iz medijev (in tudi s terena), pri statističnih in drugih analizah ipd. Za takšno obliko sodelovanja, opozorila in konstruktivne predloge nam pišite na info@sledilnik.org.
</details>

<details>
  <summary>Kako urejate podatke?</summary>

Celoten postopek zbiranja in urejanja podatkov je opisan na strani [O projektu](https://covid-19.sledilnik.org/#/about).
</details>

<details>
  <summary>Kako lahko pridobim in uporabim vašo bazo podatkov?</summary>

Naša baza podatkov je javna in prosto dostopna v obliki **CSV**, **REST** in **Google Sheet**. Prosimo vas le, da nam sporočite, s kakšnim namenom boste podatke uporabili, ter Sledilnik obvezno navedete kot vir.

Ker so oznake podatkov tudi v angleščini (gl. vprašanje Ali obstaja vaša stran tudi v angleščini?), je mogoča tudi njihova mednarodna uporaba (izvoz, prikaz).
</details>


## O izračunih in grafih

<details>
  <summary>Lahko uporabim vaše grafe na svoji strani? Kako?</summary>
    
Lahko! Na svojo spletno stran lahko vgradite poljuben graf ali prikaz – ob navedbi vira, seveda. [Kliknite sem](#/embed) in s seznama izberite graf, ki ga želite vgraditi. O uporabi nas obvestite (info@sledilnik.org) in povezavo bomo z veseljem dodali tudi v našo zbirko [priporočenih povezav]([https://covid-19.sledilnik.org/#/links](https://covid-19.sledilnik.org/#/links)). 
</details>

<details>
  <summary>Kaj pomenijo odstotki, ki se pojavljajo v informativnih okvirčkih na vrhu strani?</summary>

Gre za odstotno stopnjo rasti na današnji dan v številu oseb glede na prejšnji dan. Če je, recimo, včeraj bilo v intenzivni enoti 16 oseb, danes pa so sprejeli še štiri, je to 25 % več glede na včerajšnje stanje.  
</details>

<details>
  <summary>Kaj kaže graf "Širjenje COVID-19 v Sloveniji"?</summary>

Graf prikazuje dnevno in skupno dinamiko širjenja okužbe od začetka do danes. Uporabljeni kazalniki (gl. Katere kazalnike vključuje graf o širjenju?) nam pomagajo razumeti, ali in kako uspešno obvladujemo širjenje virusa. Spremljamo lahko, kakšen je dnevni prirast okuženih, in posredno vidimo, ali ukrepi delujejo; iz podatka o številu hospitaliziranih in deleža teh v intenzivni enoti lahko razberemo, koliko oseb je bolezen resno ogrozila, hkrati pa nam ti podatki kažejo tudi, kolikšna je obremenjenost zdravstvenega sistema.

Spodaj na časovnem traku so označene prelomne točke: od prvega potrjenega primera (4. 3. 2020) do ukrepov, sprejetih za zajezitev širjenja, kot so si sledili: Slovenija uvede vstopne točke za testiranje, zapre mejo z Italijo …, kar nam pomaga spremljati dinamiko spremenljivk glede na ukrepe.  
</details>

  
<details>
  <summary>Katere kazalnike vključuje graf "Širjenje COVID-19 v Sloveniji"?</summary>

* **Testiranja (na dan)** = Število opravljenih testiranj na prisotnost virusa SARS-CoV-2, ki povzroča bolezen COVID-19. V prvih fazah epidemije je to bil pomemben pokazatelj razširjenosti virusa, a se je s spremembo metodologije testiranja oz. vzorca testiranih to spremenilo v kazalec kapacitete zdravstvenega oz. diagnostičnega sistema.
* **Testiranja (skupaj)** = Vsota testiranj do dne; podatek je uporaben v smislu primerjave oz. deleža celotne populacije, vendar je zavajajoč, saj so določene osebe lahko testirane večkrat (npr. zdravstveni delavci, zaposleni v DSO ipd.).
* **Potrjeno okuženi (na dan)** = Število potrjeno okuženih na dan. Ta kazalec ne odraža dejanskega gibanja novih okuženih v populaciji, saj se s testi ne vzorči celotne populacije, ampak se ciljno testira rizične in poklicne skupine.
* **Hospitalizirani (trenutno)** = Trenutno število oseb v bolnišnični oskrbi (na navadnem oddelku ali v enoti za intenzivno terapijo).
* **Hospitalizirani (skupaj)** = Vsota sprejetih v bolnišnico do dne.
* **V intenzivni enoti (trenutno)** = Trenutno število oseb v enotah intenzivne terapije.
* **Respirator / kritično stanje (trenutno)** = Trenutno število oseb, ki za dihanje potrebujejo respirator (medicinski ventilator).
* **Odpuščeni iz bolnišnice (na dan)** = Število odpuščenih iz bolnišnice na ta dan.
* **Odpuščeni iz bolnišnice (skupaj)** = Vsota odpuščenih iz bolnišnice do tega dne.
* **Umrli (na dan)** = Število umrlih za posledicami COVID-19 na ta dan.
* **Umrli (skupaj)** = Vsota umrlih do tega dne.
</details>


<details>
  <summary>Kaj pomeni izraz …? </summary>

* **potrjeno okuženi** = To je število oseb, ki so bile pozitivne na testu prisotnosti virusa SARS-CoV-2. Ker je število potrjeno okuženih oseb odvisno zgolj od testiranja in ker zaradi spremenjene politike testiranja večina okuženih z blagimi simptomi sploh ne bo testirana na prisotnost COVID-19, je podatek o potrjeno okuženih bistveno manjši od dejanskega števila okuženih ljudi.

* **hospitalizirani** = To je število okuženih oseb, ki imajo tako resne simptome bolezni COVID-19, da so bile sprejete v bolnišnično oskrbo. 

* **v intenzivni enoti** = Označuje število hospitaliziranih oseb, ki so zaradi simptomov bolezni COVID-19 v življenjski nevarnosti in potrebujejo namestitev v enoti za intenzivno terapijo. Gre za podmnožico kategorije *Hospitalizirani*. 

* **respirator / kritično stanje** = Označuje število hospitaliziranih oseb v intenzivni enoti, ki za dihanje potrebujejo respirator (medicinski ventilator). Gre za podmnožico kategorije *V intenzivni negi* in kategorije *Hospitalizirani*.

* **ozdraveli** = To je število oseb, ki so bile v bolnišnični oskrbi, a nimajo več simptomov bolezni in je bil po 14 dneh test za okužbo negativen (ne kaže več prisotnosti virusa).
</details>


<details>
  <summary>Kaj pomenijo različne faze (faze 1–faza 4), ki jih vidimo v grafih?</summary>

Navpične črte delijo faze, zamejene z datumi, ko so odgovorni organi spremenili način zbiranja informacij o širjenju okužbe (spremeni se način testiranja, uvedejo se interventni ukrepi samoizolacije, prepovedi zbiranja in gibanja oseb ter obvezne nošnje osnovne zaščite).
 
Faze so prikazane zato, ker se je s spremembo metodologije testiranja spremenil tudi pomen določenih kazalcev, po katerih lahko presojamo razširjenost okužb.
* **Faza 1 (4.–12. marec 2020)**: Zabeleženi so prvi primeri okužbe pri nas. Sledi se vsem primerom, testirajo se vsi kontakti. 

* **Faza 2 (13.–19. marec 2020)**: Spremeni se [metodologija testiranja](https://www.gov.si/novice/2020-03-14-spremenjeno-diagnosticiranje-za-realnejse-nacrtovanje-ukrepov-za-obvladovanje-epidemije/), uvedejo se interventni ukrepi o samoizolaciji in socialnem distanciranju.
 
* **Faza 3 (20. marec–7.april)**: Ponovno [se spremeni metodologija testiranja](https://www.gov.si/novice/2020-03-22-ministrstvo-za-zdravje-z-vrsto-ukrepov-v-boju-proti-covid-19/), vzpostavi se prepoved zbiranja več kot petih oseb na javnih površinah.

* **Faza 4 (8. april–danes)**: Nova [sprememba metodologije testiranja](https://www.gov.si/assets/ministrstva/MZ/DOKUMENTI/Koronavirus/145-Dopolnitev-navodil-glede-testiranja-na-COVID-19.pdf) - dodatno se testirajo tudi osebe z blagimi simptomi iz gospodinjstev, kjer je več oseb z okužbo dihal.
</details>

<details>
  <summary>Kaj nam pove graf "Obravnava hospitaliziranih"?</summary>

Graf ima dva prikaza, eden nam kaže število oseb v bolnišnični oskrbi na ta dan po bolnišnicah, če pa pogled spremenimo s klikom na Obravnava po pacientih, vidimo celotno sliko hospitalizacij glede na stanje pacientov: kolikšno število hospitaliziranih je v enoti intenzivne nege, koliko od teh je v kritičnem stanju in potrebuje respirator, koliko je odpuščenih in umrlih. 

To je lahko osnova za presojo bolnišničnih zmogljivosti in načrtovanje morebitnega povečanja zmogljivosti. Po besedah ministra za zdravje Tomaža Gantarja: "Za bolnike s COVID-19 imamo v bolnišnicah pripravljenih 539 postelj, po potrebi se ta zmogljivost lahko poveča do 1000 postelj, ... Za intenzivno nego imamo trenutno na razpolago 56 postelj." Če vemo, da traja hospitalizacija nekoga v intenzivni enoti pri nas pribl. 14 dni ([po besedah dr. Matjaža Jereba](https://www.rtvslo.si/zdravje/novi-koronavirus/matjaz-jereb-smrtnost-kriticno-bolnih-na-oddelku-ni-velika/519962); svetovno povprečje je 3–6 tednov), lahko graf ponudi dober uvid o obremenitvi bolnišnic. 
</details>

<details>
  <summary>Zakaj kaže graf "Prirast potrjeno okuženih" primerjavo z Južno Korejo?</summary>

Če na grafu izberemo pogled **Eksponentna rast v dnevih**, lahko vidimo povprečje rasti v istem časovnem obdobju tudi za Južno Korejo. To smo izbrali za primerjavo zato, ker ji je kljub močnemu izbruhu bolezni COVID-19 uspelo z različnimi metodami “sploščiti krivuljo” oz. povedano drugače – Južna Koreja je ena najuspešnejših držav pri obvladovanju epidemije.
</details>


<details>
  <summary>Kaj pomenijo različni prikazi dnevnega prirasta v grafu "Prirast potrjeno okuženih"?</summary>

* **Absolutni dnevni prirast** prikazuje število novih primerov potrjeno okuženih na določen dan.

* **Relativni dnevni prirast** prikazuje odstotno vrednost novih potrjeno okuženih na določen dan.

* **Eksponentna rast v dnevih** prikazuje faktor, v koliko dneh se število potrjeno okuženih podvoji.
</details>

<details>
  <summary>Kaj pomenijo zaprti primeri in kaj so aktivni primeri? </summary>

**Zaprti primeri** so seštevek vseh potrjeno okuženih, ki niso več okuženi z virusom, torej ozdravljenih oseb in mrtvih.

**Aktivni primeri** pomenijo vse potrjene okužbe z virusom, ki so še vedno aktualne (osebe virus še vedno prebolevajo).
</details>

<details>
  <summary>Kako se izračunava “podvojitev v N dneh” in kaj pomeni?</summary>
  
Na prikazu **Potrjeno okuženi po občinah** je vključena ocena **Podvojitev v N dneh**, ki pomeni, da se bo število okuženih v določeni občini predvidoma podvojilo v navedenem številu dni. To je ocena povprečne hitrosti eksponentnega naraščanja, ki temelji na podatkih iz prejšnjih dni, tako da se ugotovi dan, ko se je vrednost prepolovila.

</details>


<details>
  <summary>So vaši grafi slika realnega stanja?</summary>

Da, kolikor so lahko, če se zavedamo omejitev trenutnih prikazov: grafi na tej strani prikazujejo le tisto, kar je mogoče ugotoviti glede na dane podatke. Tako recimo skupno število testiranj pomeni število vseh opravljenih testov do danes, ne izraža pa skupnega števila vseh testiranih oseb, saj so nekatere osebe, na primer zdravstveni delavci in osebe, pri katerih sumijo na okužbo, testirane večkrat.

Po drugi strani je število potrjeno okuženih oseb odvisno zgolj od testiranja, in ker zaradi spremenjene politike testiranja večina okuženih z blagimi simptomi sploh ne bo testirana na prisotnost COVID-19, je podatek o potrjeno okuženih bistveno manjši od dejanskega števila okuženih ljudi.

Zato je treba te kategorije jemati z védenjem, kaj pomenijo, in interpretirati grafe z zrncem soli.
</details>


<details>
  <summary>Kako računate odstotni (%) prirast? </summary>

Za odstotni prirast vzamemo trenutno vrednost spremenljivke in od nje odštejemo stanje prejšnjega dne. Dobljeno razliko delimo s stanjem prejšnjega dne in jo pomnožimo s 100, da dobimo odstotni prirast, ki ga za potrebe predstavitve zaokrožimo na eno decimalko natančno.

Zavedamo se, da obstajajo drugačne metode, ki odstotni prirast prikazujejo drugače, vendar se nam je uporabljena metoda zdela za naše razmere in namen najprimernejša in najlažje razumljiva.
</details>


<details>
  <summary>Kaj pomeni logaritemska skala na Y osi in kako deluje?</summary>

Logaritemska skala na navpični osi (ordinata, Y os) je izjemno uporabna za prikaz funkcij oz. količin, ki zelo hitro naraščajo – recimo za t.i. eksponentno rast okuženih –, saj bi v navadnem merilu hitro prerasla najvišjo vrednost na ordinatni osi. 
</details>

<details>
  <summary>Kaj pomeni “eksponentna rast okuženih”? Kako lahko merimo, s kakšno hitrostjo se širi epidemija?</summary>

Pri epidemijah nalezljivih bolezni je zelo pomembna hitrost širjenja oz. stopnja rasti okužb, saj to vpliva tudi na število obolelih in smrti. Če se število okužb v nekem določenem času povečuje za enako število, npr. za 10 vsake tri dni – 10, 20, 30, 40 ..., gre za *linearno rast primerov*; če pa se število okužb v določenem časovnem obdobju podvoji, recimo podvojitev za 10 vsake 3 dni – 10, 20, 40, 80 …, pa govorimo o *eksponentni rasti*, ki v kratkem času privede do zelo velikega števila obolelih.

Čas podvojitve kot kazalec hitrosti širjenja epidemije se spreminja (pada, raste), zato ga ne smemo preprosto projicirati v prihodnost; kaže nam zgolj trenutno hitrost podvajanja primerov na podlagi podatkov iz preteklosti.
</details>


## O projektu

<details>
  <summary>Kaj je Sledilnik?</summary>

[Sledilnik je projekt](https://covid-19.sledilnik.org/#/about), ki zbira, analizira in prikazuje nekaj najbolj uporabnih podatkov, da bi lahko bolje razumeli širjenje pandemije koronavirusa in bolezni COVID-19 skupaj z njeno dinamiko in obsegom. 

Želimo si jasno predstaviti, kaj nam trenutni podatki in pregledi govorijo o širjenju virusa v Sloveniji, in zagotoviti, da postanejo informacije o obsegu in resnosti problema COVID-19 v Sloveniji vsem dostopne in čim bolj razumljive. 
</details>


<details>
  <summary>Imam odlično povezavo na stran, ki je vi še nimate v povezavah, pa bi si tam zaslužila biti. Jo boste dodali?</summary>

Pišite nam na info@sledilnik.org – predlagano povezavo bomo preverili in jo, če je stran verodostojna in koristna, z veseljem vključili med naše povezave.

Če želite narediti še korak dlje in prispevati k skupnemu cilju, nam na [GitHubu](https://github.com/slo-covid-19/website/blob/master/src/content/links.md) oddajte Pull-Request (PR).</p>
</details>


<details>
  <summary>Želim pomagati, kje lahko začnem?</summary>

Pišite nam na info@sledilnik.org in na kratko opišite, kdo ste in kako lahko prispevate k projektu. Vabljeni!
</details>



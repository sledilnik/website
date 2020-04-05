<h1>FAQ - Pogosta vprašanja in odgovori</h1>

_Namen Sledilnikovih prikazov je združiti in predstaviti obstoječe podatke v vsakomur razumljivi obliki, ki pa vseeno potrebujejo določena pojasnila. Zbrali smo odgovore na nekatera najpogostejša vprašanja. Seznam sproti dopolnjujemo; če imate kakršna koli vprašanja glede naših prikazov (ali predlog za boljše pojasnilo) in jih ne najdete v FAQ, nam pišite na info@sledilnik.org. (Veseli smo tudi pohval, povratnih informacij ali predlogov!)_

<details>
  <summary>So vaši podatki in prikazi verodostojni?</summary>
  
Podatke zbiramo iz različnih uradnih in drugih javnih virov - navedeni so v [zavihku Viri](https://covid-19.sledilnik.org/#/sources). 

Od 28. marca 2020 imamo vzpostavljeno tudi povezavo z Ministrstvom za zdravje, NIJZ in zdravstvenimi zavodi, od katerih zdaj neposredno dobivamo strukturirane podatke. Ekipa Sledilnika ne nadzoruje točnosti izvirnih podatkov in ne objavlja podatkov, ki niso pridobljeni iz uradnih virov ali sredstev javnega obveščanja, zato pa vse podatke navzkrižno preverja, da so pravilni in skladni z izvornimi.
</details>


<details>
  <summary>Kako se izračunava “podvojitev v N dneh” in kaj pomeni?</summary>
  
Na prikazu **Potrjeno okuženi po občinah** je prikazana ocena **Podvojitev v N dneh**. To je ocena povprečne hitrosti eksponentnega naraščanja skupnega števila potrjeno okuženih, izračunana na podlagi spremembe v zadnjih 6 dneh. Povprečje uporabimo zato, da zgladimo dnevno nihanje pri potrjenih primerih, in se izognemo zaokrožitvam zaradi (na srečo) majhnih absolutnih številk. Nižja, kot je številka, hitreje se je število potrjeno okuženih povečalo.

Za izračun uporabimo naslednji izraz: `čas_podvojitve = 6 / log2( P0 / P6 )</code>`. V izrazu `Pd` pomeni število potrjeno okuženih v občini pred `d` dnevi.
</details>


<details>
  <summary>Kdaj objavljate podatke? Zakaj so nekateri podatki osveženi z današnjim dnem, drugi pa imajo včerajšnji datum?</summary>

Večina podatkov se zbira za pretekli dan ob 23:59 (testi, potrjene okužbe,...), podatke o hospitalizacijah pa večinoma pridobimo do 9:00 vsak dan za vse bolnišnice. **Naši podatki so tako osveženi ponavadi med 10:00 in 12:00**.  

Ko objavimo sveže dnevne podatke, so objavljeni na vseh naših distribucijskih poteh (CSV, REST, spletna stran) in objavimo poročilo na družbenih omrežjih ([Facebook](https://www.facebook.com/COVID19Sledilnik) in [Twitter](https://twitter.com/sledilnik)).
</details>


<details>
  <summary>Kako se lahko uporabniki aktivno vključimo v oddajo podatkov? Kako lahko sodelujem?</summary>

Sledilnik ne zbira osebnih podatkov uporabnikov, niti podatkov, ki bi jih želeli o svojem stanju ali o stanju v bolnišnicah posredovati posamezniki.

Lahko pa uporabniki prostovoljno pomagate z zbiranjem in preverjanjem podatkov iz medijev (in tudi s terena), pri statističnih in drugih analizah ipd. Za takšno obliko sodelovanja, opozorila in konstruktivne predloge nam pišite na info@sledilnik.org.
</details>


<details>
  <summary>Kje lahko najdem primerjavo med Slovenijo in drugimi državami?</summary>

Sledilnik trenutno ne prikazuje nobenih vizualizacij, ki bi stanje v Sloveniji primerjale s podobnimi stanji v tujini. Za takšne primerjave si lahko vedno ogledate katero od strani, kot je [Worldometer](https://www.worldometers.info/coronavirus/) oz. preverite na [strani Povezave](https://covid-19.sledilnik.org/#/links).
</details>


<details>
  <summary>Kako pridobivate podatke o odpuščenih iz bolnišnic?</summary>

Podatek **Odpuščeni iz bolnišnice** je Sledilnikova ocena, izračunana na podlagi podatkov, ki jih dnevno dobivamo neposredno iz bolnišnic, torej iz preverjenega vira. Ker pa bolnišnice ne poročajo vseh sprejemov in odpustov iz bolnišnice, je naša ocena narejena na podlagi spremembe trenutno hospitaliziranih pacientov (če trenutno število pacientov pade, sklepamo da so bili odpuščeni). Iz tega razloga je naša ocena konzervativna (nižja od dejanskega števila odpuščenih bolnikov).
</details>


<details>
  <summary>Zakaj v vizualizacijah bolniki v intenzivni enoti, ki potrebujejo respirator, tako odstopajo od tistih, ki ga ne potrebujejo?</summary>

Bolnikov v intenzivni enoti na začetku zaradi manka podatkov nismo mogli z gotovostjo ločiti na bolnikov, ki so respirator potrebovali, in tiste, ki ga k sreči niso. V zadnjem času prejemamo natančna poročila o uporabi respiratorja, zato so tudi poročila o respiratorju zelo zanesljiva.
</details>


<details>
  <summary>Kaj pomenijo različne faze (faze 1 - faza 3), ki jih vidimo na vizualizacijah?</summary>

Navpične črte delijo faze, zamejene z datumi, ko so odgovorni organi spremenili način zbiranja informacij o širjenju okužbe (spremeni se način testiranja, uvedejo se interventni ukrepi samoizolacije, prepovedi zbiranja in gibanja oseb ter obvezne nošnje osnovne zaščite).
 
Faze so prikazane zato, ker se je s spremembo metodologije testiranja spremenil tudi pomen določenih kazalcev, po katerih lahko presojamo razširjenost okužb.
* **Faza 1 (4. – 12. marec 2020)**: Zabeleženi so prvi primeri okužbe pri nas. Sledi se vsem primerom, testirajo se vsi kontakti. 

* **Faza 2 (13. – 19. marec 2020)**: Spremeni [se metodologija testiranja](https://www.gov.si/novice/2020-03-14-spremenjeno-diagnosticiranje-za-realnejse-nacrtovanje-ukrepov-za-obvladovanje-epidemije/), uvedejo se interventni ukrepi o samoizolaciji in socialnem distanciranju.
 
* **Faza 3 (20. marec – danes)**: Ponovno [se spremeni metodologija testiranja](https://www.gov.si/novice/2020-03-22-ministrstvo-za-zdravje-z-vrsto-ukrepov-v-boju-proti-covid-19/), vzpostavi se prepoved zbiranja več kot petih oseb na javnih površinah.
</details>


<details>
  <summary>Zakaj je na vizualizaciji "Hitrost širjenja okužbe" primerjava z Južno Korejo?</summary>

Če na grafu izberemo pogled **Eksponentna rast v dnevih**, lahko vidimo povprečje rasti v istem časovnem obdobju tudi za Južno Korejo. To smo izbrali za primerjavo zato, ker ji je kljub močnemu izbruhu bolezni COVID-19 uspelo z različnimi metodami “sploščiti krivuljo” oz. povedano drugače – Južna Koreja je ena najuspešnejših držav pri obvladovanju epidemije.
</details>


<details>
  <summary>Kaj pomenijo zaprti primeri in kaj so aktivni primeri? </summary>

**Zaprti primeri** so seštevek vseh potrjeno okuženih, ki niso več okuženi z virusom, torej ozdravljenih oseb in mrtvih.

**Aktivni primeri** pomenijo vse potrjene okužbe z virusom, ki so še vedno aktualne (osebe virus še vedno prebolevajo).
</details>


<details>
  <summary>Kakšna je razlika med SARS-Cov-2 in COVID-19?</summary>

**SARS-CoV-2** je angleška okrajšava za “Severe Acute Respiratory Syndrome Coronavirus 2” – to je mednarodno sprejeto ime virusa, ki povzroča bolezen **COVID-19**. Tudi slednje poimenovanje je kratica, skovana iz besed COrona VIrus Disease ter 2019, torej leta, ko je bolezen prvič izbruhnila.
</details>


<details>
  <summary>Kako urejate podatke?</summary>

Celoten postopek zbiranja in urejanja podatkov je opisan na strani [O projektu](https://covid-19.sledilnik.org/#/about).
</details>


<details>
  <summary>Ali obstaja vaša stran tudi v angleščini?</summary>

Zaenkrat ne, sta pa na voljo za prosto uporabo tako besedilni del, kot izvorna koda, če bi se želel kdo lotiti tega podviga. Vsi podatki so v bazi že zavedeni tudi z angleškimi oznakami, zato je mogoča tudi njihova mednarodna uporaba (izvoz). </details>


<details>
  <summary>Kako lahko pridobim in uporabim vašo bazo podatkov?</summary>

Naša baza podatkov je javna in prosto dostopna v obliki **CSV**, **REST** in **Google Sheet**. Prosimo vas le, da nam sporočite, s kakšnim namenom boste podatke uporabili, ter Sledilnik obvezno navedete kot vir.

Ker so oznake podatkov tudi v angleščini (gl. vprašanje Ali obstaja vaša stran tudi v angleščini?), je mogoča tudi njihova mednarodna uporaba (izvoz, prikaz).
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
  <summary>Kaj pomenijo različni prikazi dnevnega prirasta v vizualizaciji Hitrost širjenja okužbe?</summary>

**Absolutni dnevni prirast** prikazuje število novih primerov potrjeno okuženih na določen dan.

**Relativni dnevni prirast** prikazuje odstotna vrednost novih potrjeno okuženih na določen dan.

**Eksponentna rast v dnevih** pa prikazuje prikazuje faktor, v koliko dneh se število potrjeno okuženih podvoji.
</details>


<details>
  <summary>Kaj pomeni izraz … </summary>

* **potrjeno okuženi** = To je število oseb, ki so bile pozitivne na testu prisotnosti virusa SARS-CoV-2. Ker je število potrjeno okuženih oseb odvisno zgolj od testiranja, in ker zaradi spremenjene politike testiranja večina okuženih z blagimi simptomi sploh ne bo testirana na prisotnost COVID-19, je podatek o potrjeno okuženih bistveno manjši od dejanskega števila okuženih ljudi.

* **hospitalizirani** = To je število okuženih oseb, ki imajo tako resne simptome bolezni COVID-19, da so bile sprejete v bolnišnično oskrbo. 

* **v intenzivni enoti** = Označuje število hospitaliziranih oseb, ki so zaradi simptomov bolezni COVID-19 v življenjski nevarnosti in potrebujejo namestitev v enoti za intenzivno terapijo. Gre za podmnožico kategorije Hospitalizirani. 

* **respirator / kritično stanje** = Označuje število hospitaliziranih oseb v intenzivni enoti, ki za dihanje potrebujejo respirator (medicinski ventilaror). Gre za podmnožico kategorije *V intenzivni negi* in kategorije *Hospitalizirani*.

* **ozdraveli** = To je število oseb, ki so bile v bolnišnični oskrbi, a nimajo več simptomov bolezni in je bil po 14 dneh test za okužbo negativen (ne kaže več prisotnosti virusa).
</details>


<details>
  <summary>Kaj je Sledilnik?</summary>

[Sledilnik je projekt](https://covid-19.sledilnik.org/#/about), ki zbira, analizira in prikazuje nekaj najbolj uporabnih podatkov, da bi lahko bolje razumeli širjenje pandemije koronavirusa in bolezni COVID-19 skupaj z njeno dinamiko in obsegom. 

Želimo si jasno predstaviti, kaj nam trenutni podatki in pregledi govorijo o širjenju virusa v Sloveniji, in zagotoviti, da postanejo informacije o obsegu in resnosti problema COVID-19 v Sloveniji vsem dostopne in čim bolj razumljive. 
</details>


<details>
  <summary>Želim pomagati, kje lahko začnem?</summary>

Pišite nam na info@sledilnik.org in na kratko opišite, kdo ste in kako lahko prispevate k projektu. Vabljeni!
</details>


<details>
  <summary>Na drugih portalih so številke obolelih drugačne kot pri vas. Zakaj?</summary>

Sledilnik uporablja zgolj potrjene, uradne podatke, ki jih dnevno sporočajo NIJZ in vse slovenske bolnišnice, ki zdravijo bolezen COVID-19. Naši podatki tako prihajajo neposredno iz preverjenih virov, hkrati pa jih tudi sami navzkrižno primerjamo že od začetka delovanja (4.3.2020). Razlike v objavljenih podatki ponavadi prihajajo, zato ker so bili zajeti ob različni uri dneva.

(Gl. tudi vprašanje So podatki verodostojni?)</p>
</details>


<details>
  <summary>Imam odlično povezavo na stran, ki je vi še nimate v povezavah, pa bi si tam zaslužila biti. Jo boste dodali?</summary>

Pišite nam na info@sledilnik.org - predlagano povezavo bomo preverili in jo, če je stran verodostojna in koristna, z veseljem vključili med naše povezave.

Če želite narediti še korak dlje in prispevati k skupnemu cilju, nam na [GitHubu](https://github.com/slo-covid-19/website/blob/master/src/content/links.md) oddajte Pull-Request (PR).</p>
</details>

## OPISI GRAFOV

<details>
  <summary>Kaj pomeni/prikazuje prikaz/graf ______________</summary>

* **Testiranja (na dan)** = Število opravljenih testiranj na prisotnost virusa SARS-CoV-2, ki povzroča bolezen COVID-19.  V prvih fazah epidemije je to bil pomemben pokazatelj razširjenosti virusa, a se je s spremembo metodologije testiranja oz. vzorca testiranih to spremenilo v kazalec kapacitete zdravstvenega oz. diagnostičnega sistema.

* **Testiranja (skupaj)** = Vsota testiranj do dne; podatek je uporaben v smislu primerjave oz. deleža celotne populacije, vendar je zavajajoč, saj so določene osebe lahko testirane večkrat (npr. zdravstveni delavci, zaposleni v DSO...).
* **Potrjeno okuženi (na dan)** = Število potrjeno okuženih na dan. Ta kazalec ne odraža dejanskega gibanja novih okuženih v populaciji, saj s testi ne vzorčijo celotne populacije, ampak se ciljno testira rizične in poklicne skupine.
* **Hospitalizirani (trenutno)** = Trenutno število oseb v bolnišnični oskrbi (na navadnem oddelku ali enoti za intenzivno terapijo).
* **Hospitalizirani (skupaj)** = Vsota sprejetih v bolnišnico do dne.
* **V intenzivni enoti (trenutno)** = Trenutno število oseb v enotah intenzivne terapije.
* **Respirator / kritično stanje (trenutno)** = Trenutno število oseb, ki za dihanje potrebujejo respirator (medicinski ventilaror).
* **Odpuščeni iz bolnišnice (na dan)** = Število odpuščenih iz bolnišnice na ta dan.
* **Odpuščeni iz bolnišnice (skupaj)** = Vsota odpuščenih iz bolnišnice do tega dne.
* **Umrli (na dan)** = Število umrlih za posledicami COVID-19 na ta dan.
* **Umrli (skupaj)** = Vsota umrlih do tega dne.
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

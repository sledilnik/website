<h1>FAQ - Pogosta vprašanja in odgovori</h1>

_Namen Sledilnikovih prikazov je združiti in predstaviti obstoječe podatke v vsakomur razumljivi obliki, ki pa vseeno potrebujejo določena pojasnila. Zbrali smo odgovore na nekatera najpogostejša vprašanja. Seznam sproti dopolnjujemo; če imate kakršna koli vprašanja glede naših prikazov (ali predlog za boljše pojasnilo) in jih ne najdete v FAQ, nam pišite na info@sledilnik.org. (Veseli smo tudi pohval, povratnih informacij ali predlogov!)_

[comment]: <> (!!!!!Vse pod tem izbrisi!!!!!)

<details>
  <summary>Primer  !!!!!IZBRISI!!!!!</summary>
  
  Pod _summary obvezno prazna vsrtica_

To je nek text [Link](https://www.Sub.bullets.com) **boldan text**

</details>

<div class="dropdown">
  <div class="dd-title">To je dropdown s HTML  !!!!!IZBRISI!!!!!</div>
  <div class="dd-content">
    <p>Podatke zbiramo iz različnih uradnih <a href="https://www.nijz.si/sl/dnevno-spremljanje-okuzb-s-sars-cov-2-covid-19" target="_blank">in drugih javnih virov</a>. Od 28. marca 2020 imamo vzpostavljeno tudi povezavo z zdravstvenimi zavodi in NIJZ, od katerih zdaj neposredno dobivamo strukturirane podatke. Ekipa Sledilnika ne nadzoruje točnosti izvirnih podatkov in ne objavlja podatkov, ki niso pridobljeni iz uradnih virov ali sredstev javnega obveščanja, zato pa vse podatke navzkrižno preverja, da so pravilni in skladni z izvornimi.</p>
  </div>
</div>

<details>
  <summary>To je DD z MD  !!!IZBRISI!!!!</summary>
  
  V naši skupnosti sodelujejo tudi strokovnjaki za statistično modeliranje in računalniške simulacije.
Na tej strani objavljamo povezave na nekatere od njihovih modelov,
ki so jih pripravili in umerili z uporabo podatkov [COVID-19 Sledilnik](covid-19.sledilnik.org).

Modeli v veliki meri upoštevajo doslej znane informacije o bolezni COVID-19 in njenem širjenju v Sloveniji, a kljub temu **ne ponujajo natančnih napovedi** za prihodnji potek epidemije, **zato je potrebno skrbno prebrati vse predpostavke modela**.
Podatki o testih in potrjenih okuženih osebah v Sloveniji so skopi, zato sta oba predstavljena modela umerjena na podatke o hospitalizacijah. Pomanjkanje natančnih podatkov o primerih je eden glavnih razlogov za nedoločenosti.

Svetovna znanost vlaga velike napore v boj z boleznijo COVID-19, a vendar mnogi vidiki širjenja in razvoja bolezni še niso raziskani. Še posebno velika je negotovost glede učinka ukrepov, ki jih vlade po svetu izvajajo za omejitev bolezni. Poleg tega je, zaradi časovnih zamikov med okužbo in potrditvijo, praktično nemogoče točno oceniti dejansko stanje okuženosti in hitrost širjenja okužbe v populaciji. Vse to so razlogi, da so modelom pripadajoči intervali nedoločenosti sorazmerno veliki, in, dlje kot gledamo v prihodnost, hitreje rastejo.

</details>

[comment]: <> (!!!!!Vse nad tem izbrisi!!!!)

<details>
  <summary>So vaši podatki in prikazi verodostojni?</summary>
  
Podatke zbiramo iz različnih uradnih in drugih javnih virov - navedeni so v [zavihku Viri](#/sources). Od 28. marca 2020 imamo vzpostavljeno tudi povezavo z Ministrstvom za zdravje, NIJZ in zdravstvenimi zavodi, od katerih zdaj neposredno dobivamo strukturirane podatke. Ekipa Sledilnika ne nadzoruje točnosti izvirnih podatkov in ne objavlja podatkov, ki niso pridobljeni iz uradnih virov ali sredstev javnega obveščanja, zato pa vse podatke navzkrižno preverja, da so pravilni in skladni z izvornimi.
</details>

<details>
  <summary>Kako se izračunava “podvojitev v N dneh” in kaj pomeni?</summary>
  
Na prikazu "**Potrjeno okuženi po občinah**" je prikazana ocena "**Podvojitev v N dneh**". To je ocena povprečne hitrosti eksponentnega naraščanja skupnega števila potrjeno okuženih, izračunana na podlagi spremembe v zadnjih 6 dneh. Povprečje uporabimo zato, da zgladimo dnevno nihanje pri potrjenih primerih, in se izognemo zaokrožitvam zaradi (na srečo) majhnih absolutnih številk. Nižja, kot je številka, hitreje se je število potrjeno okuženih povečalo.

Za izračun uporabimo naslednji izraz: `čas_podvojitve = 6 / log2(P0/P6)</code>`. V izrazu `Pd` pomeni število potrjeno okuženih v občini pred `d` dnevi.
</details>

<div class="dropdown">
  <div class="dd-title">Kdaj objavljate podatke? Zakaj so nekateri podatki osveženi z današnjim dnem, drugi pa imajo včerajšnji datum?</div>
  <div class="dd-content">    
   <p>Večina podatkov se zbira za pretekli dan ob 23:59 (testi, potrjene okužbe,...), podatke o hospitalizacijah pa večinoma pridobimo do 9:00 vsak dan za vse bolnišnice. Naši podatki so tako osveženi ponavadi med 10:00 in 12:00. </p> 
   <p>Ko objavimo sveže dnevne podatke, so objavljeni v vseh naših distribucijskih poteh (CSV, REST, spletna stran) in objavimo poročilo na družbenih omrežjih (Facebook in Twitter).</p>
  </div>
</div>
<div class="dropdown">
  <div class="dd-title">Kako se lahko uporabniki aktivno vključimo v oddajo podatkov? Kako lahko sodelujem?</div>
  <div class="dd-content">
    <p>Sledilnik ne zbira osebnih podatkov uporabnikov, niti podatkov, ki bi jih želeli o svojem stanju ali o stanju v bolnišnicah posredovati posamezniki.</p>
    <p>Lahko pa uporabniki prostovoljno pomagate z zbiranjem in preverjanjem podatkov iz medijev (in tudi s terena), pri statističnih in drugih analizah ipd. Za takšno obliko sodelovanja, opozorila in konstruktivne predloge nam pišite na info@sledilnik.org.</p>
  </div>
</div>

<div class="dropdown">
 <div class="dd-title">Kje lahko najdem primerjavo med Slovenijo in drugimi državami? 
</div>
<div class="dd-content">
 <p>Sledilnik trenutno ne prikazuje nobenih vizualizacij, ki bi stanje v Sloveniji primerjale s podobnimi stanji v tujini. Za takšne primerjave si lahko vedno ogledate katero od strani, kot je Worldometer oz. preverite na strani Povezave.</p>
</div>
</div>

<div class="dropdown">
 <div class="dd-title">Kako pridobivate podatke o odpuščenih iz bolnišnic?
</div>
<div class="dd-content">
 <p>Podatek Odpuščeni iz bolnišnice je Sledilnikova ocena, izračunana na podlagi podatkov, ki jih dnevno dobivamo neposredno iz bolnišnic, torej iz preverjenega vira. Ker pa bolnišnice ne poročajo vseh sprejemov in odpustov iz bolnišnice, je naša ocena narejena na podlagi spremembe trenutno hospitaliziranih pacientov (če trenutno število pacientov pade, sklepamo da so bili odpuščeni).</p>
</div>
</div>

<div class="dropdown">
 <div class="dd-title">Zakaj v vizualizacijah bolniki v intenzivni enoti, ki potrebujejo respirator, tako odstopajo od tistih, ki ga ne potrebujejo?
</div>
<div class="dd-content">
 <p>Bolnikov v intenzivni enoti na začetku zaradi manka podatkov nismo mogli z gotovostjo ločiti na bolnike, ki so respirator potrebovali, in tiste, ki ga k sreči niso. V zadnjem času prejemamo natančna poročila o uporabi respiratorja, zato so tudi poročila o respiratorju zelo zanesljiva. </p>
</div>
</div>

<div class="dropdown">
 <div class="dd-title">Kaj pomenijo različne faze (F1–F3), ki jih vidimo na vizualizacijah?</div>
 <div class="dd-content">
 <p>Navpične črte delijo faze, zamejene z datumi, ko so odgovorni organi spremenili način zbiranja informacij o širjenju okužbe (spremeni se način testiranja, uvedejo se interventni ukrepi samoizolacije, prepovedi zbiranja in gibanja oseb ter obvezne nošnje osnovne zaščite).</p>
 <p>Faze so prikazane zato, ker se je s spremembo metodologije testiranja spremenil tudi pomen določenih kazalcev, po katerih lahko presojamo razširjenost okužb. </p>
 <p>Faza 1 (4. marec.–12. marec 2020): Zabeleženi so prvi primeri okužbe pri nas. Sledi se vsem primerom, testirajo se vsi kontakti. 
</p>
 <p>Faza 2 (13. marec–19. marec 2020): Spremeni se metodologija testiranja, uvedejo se interventni ukrepi o samoizolaciji in socialnem distanciranju.
</p>
 <p>F3 (20. marec–danes): Ponovno se spremeni metodologija testiranja, vzpostavi se prepoved zbiranja več kot petih oseb na javnih površinah.
</p>
</div>
</div>

<div class="dropdown">
 <div class="dd-title">Zakaj je na vizualizaciji "Hitrost širjenja okužbe" primerjava z Južno Korejo? </div>
 <div class="dd-content">
 <p>Če na grafu izberemo pogled Eksponentna rast v dnevih, lahko vidimo povprečje rasti v istem časovnem obdobju tudi za Južno Korejo. To smo izbrali za primerjavo zato, ker ji je kljub močnemu izbruhu bolezni COVID-19 uspelo z različnimi metodami “sploščiti krivuljo” oz. povedano drugače – Južna Koreja je ena najuspešnejših držav pri obvladovanju epidemije.</p>
</div>
</div>

<div class="dropdown">
 <div class="dd-title">Kaj pomenijo zaprti primeri in kaj so aktivni primeri?  
</div>
<div class="dd-content">
 <p>Zaprti primeri so seštevek vseh tistih, ki niso več okuženi z virusom, torej ozdravljenih oseb in mrtvih. Aktivni primeri pomenijo vse potrjene okužbe z virusom, ki so še vedno aktualne (osebe virus še vedno prebolevajo).</p>
</div>
</div>

<div class="dropdown">
 <div class="dd-title">Kakšna je razlika med SARS-Cov-2 in COVID-19?</div>
 <div class="dd-content">
 <p>SARS-CoV-2 je angleška okrajšava za “Severe Acute Respiratory Syndrome Coronavirus 2” – to je mednarodno sprejeto ime virusa, ki povzroča bolezen COVID-19. Tudi slednje poimenovanje je kratica, skovana iz besed COrona VIrus Disease ter 2019, torej leta, ko je bolezen prvič izbruhnila.</p>
</div>
</div>

<div class="dropdown">
 <div class="dd-title">Kako urejate podatke?</div>
 <div class="dd-content">
 <p>Celoten postopek zbiranja in urejanja podatkov je opisan na tej strani:  https://covid-19.sledilnik.org/#/about </p>
</div>
</div>

<div class="dropdown">
 <div class="dd-title">Ali obstaja vaša stran tudi v angleščini?</div>
 <div class="dd-content">
 <p>Zaenkrat ne, sta pa na voljo za prosto uporabo tako besedilni del, kot izvorna koda, če bi se želel kdo lotiti tega podviga. Vsi podatki so v bazi že zavedeni tudi z angleškimi oznakami, zato je mogoča tudi njihova mednarodna uporaba (izvoz). </p>
</div>
</div>

<div class="dropdown">
 <div class="dd-title">Kako lahko pridobim in uporabim vašo bazo podatkov?
 </div>
 <div class="dd-content">
 <p>Naša baza podatkov je javna in prosto dostopna v obliki CSV, REST, Google Sheet. Prosimo vas le, da nam sporočite, s kakšnim namenom boste podatke uporabili, ter Sledilnik obvezno navedete kot vir. </p>
 <p>Ker so oznake podatkov tudi v angleščini (gl. vprašanje Ali obstaja vaša stran tudi v angleščini?), je mogoča tudi njihova mednarodna uporaba (izvoz, prikaz).</p>
</div>
</div>

<div class="dropdown">
 <div class="dd-title">So vaši grafi slika realnega stanja?</div>
 <div class="dd-content">
 <p>Da, kolikor so lahko, če se zavedamo omejitev trenutnih prikazov: grafi na tej strani prikazujejo le tisto, kar je mogoče ugotoviti glede na dane podatke. Tako recimo skupno število testiranj pomeni število vseh opravljenih testov do danes, ne izraža pa skupnega števila vseh testiranih oseb, saj so nekatere osebe, na primer zdravstveni delavci in osebe, pri katerih sumijo na okužbo, testirane večkrat. </p>
 <p>Po drugi strani je število potrjeno okuženih oseb odvisno zgolj od testiranja, in ker zaradi spremenjene politike testiranja večina okuženih z blagimi simptomi sploh ne bo testirana na prisotnost COVID-19, je podatek o potrjeno okuženih bistveno manjši od dejanskega števila okuženih ljudi. </p>
 <p>Zato je treba te kategorije jemati z védenjem, kaj pomenijo, in interpretirati grafe z zrncem soli.</p>
</div>
</div>

<div class="dropdown">
 <div class="dd-title">Kako računate odstotni (%) prirast? </div>
 <div class="dd-content">
 <p>Za odstotni prirast vzamemo trenutno vrednost spremenljivke in od nje odštejemo stanje prejšnjega dne. Dobljeno razliko delimo s stanjem prejšnjega dne in jo pomnožimo s 100, da dobimo odstotni prirast, ki ga za potrebe predstavitve zaokrožimo na eno decimalko natančno.
 </p>
 <p>Zavedamo se, da obstajajo drugačne metode, ki odstotni prirast prikazujejo drugače, vendar se nam je uporabljena metoda zdela za naše razmere in namen najprimernejša in najlažje razumljiva.</p>
</div>
</div>

<div class="dropdown">
 <div class="dd-title">Kaj pomenijo različni prikazi dnevnega prirasta v vizualizaciji Hitrost širjenja okužbe?</div>
 <div class="dd-content">
 <p>Absolutni dnevni prirast prikazuje število novih primerov potrjeno okuženih na določen dan.</p>
 <p>Relativni dnevni prirast prikazuje odstotna vrednost novih potrjeno okuženih na določen dan.</p>
 <p>Eksponentna rast v dnevih pa prikazuje prikazuje faktor, v koliko dneh se število potrjeno okuženih podvoji.</p>
</div>
</div>

<div class="dropdown">
 <div class="dd-title">Kaj pomeni izraz … </div>
 <div class="dd-content">
 <p> -potrjeno okuženi = To je število oseb, ki so bile pozitivne na testu prisotnosti virusa SARS-CoV-2. Ker je število potrjeno okuženih oseb odvisno zgolj od testiranja, in ker zaradi spremenjene politike testiranja večina okuženih z blagimi simptomi sploh ne bo testirana na prisotnost COVID-19, je podatek o potrjeno okuženih bistveno manjši od dejanskega števila okuženih ljudi.
 </p>
 <p> -hospitalizirani = To je število okuženih oseb, ki imajo tako resne simptome bolezni COVID-19, da so bile sprejete v bolnišnično oskrbo. 
</p>
 <p> -v intenzivni enoti = Označuje število hospitaliziranih oseb, ki so zaradi simptomov bolezni COVID-19 v življenjski nevarnosti in potrebujejo namestitev v enoti za intenzivno terapijo. Gre za podmnožico kategorije Hospitalizirani. 
</p>
   <p> -ozdraveli = To je število oseb, ki so bile v bolnišnični oskrbi, a nimajo več simptomov bolezni in je bil po 14 dneh test za okužbo negativen (ne kaže več prisotnosti virusa).</p>

</div>
</div>

<div class="dropdown">
 <div class="dd-title">Kaj je Sledilnik?</div>
 <div class="dd-content">
 <p>Sledilnik je projekt, ki zbira, analizira in prikazuje nekaj najbolj uporabnih podatkov, da bi lahko bolje razumeli širjenje pandemije koronavirusa COVID-19 skupaj z njeno dinamiko in obsegom. Želimo si jasno predstaviti, kaj nam trenutni podatki in pregledi govorijo o širjenju virusa v Sloveniji, in zagotoviti, da postanejo informacije o obsegu in resnosti problema COVID-19 v Sloveniji vsem dostopne in čim bolj razumljive. </p>
</div>
</div>

<div class="dropdown">
 <div class="dd-title">Želim pomagati, kje lahko začnem?</div>
 <div class="dd-content">
 <p>Pišite nam na info@sledilnik.org in na kratko opišite, kdo ste in kako lahko prispevate k projektu. Vabljeni!</p>

</div>
</div>

<div class="dropdown">
 <div class="dd-title">Na drugih portalih so številke obolelih drugačne kot pri vas. Zakaj? </div>
 <div class="dd-content">
 <p>Sledilnik uporablja zgolj potrjene, uradne podatke, ki jih dnevno sporočajo NIJZ in vse slovenske bolnišnice, ki zdravijo bolezen COVID-19. Naši podatki tako prihajajo neposredno iz preverjenih virov, hkrati pa jih tudi sami navzkrižno primerjamo že od začetka delovanja (4.3.2020). </p>
 <p> (Gl. tudi vprašanje So podatki verodostojni?)</p>

</div>
</div>

<div class="dropdown">
 <div class="dd-title">Imam odlično povezavo na stran, ki je vi še nimate v povezavah, pa bi si tam zaslužila biti. Jo boste dodali?</div>
 <div class="dd-content">
 <p>Pišite nam na info@sledilnik.org - predlagano povezavo bomo preverili in jo, če je stran verodostojna in koristna, z veseljem vključili med naše povezave. </p>
 <p>Če želite narediti še korak dlje in prispevati k skupnemu cilju, nam na Githubu https://github.com/slo-covid-19/website/blob/master/src/content/links.md oddajte PR request.</p>
</div>
</div>

## OPISI GRAFOV

<div class="dropdown">
 <div class="dd-title">Kaj pomeni/prikazuje prikaz/graf ______________</div>
 <div class="dd-content">
 <p>Testiranja (na dan) = Število opravljenih testiranj na prisotnost virusa SARS-CoV-2, ki povzroča bolezen COVID-19.  V prvih fazah epidemije je to bil pomemben pokazatelj razširjenosti virusa, a se je s spremembo metodologije testiranja oz. vzorca testiranih to spremenilo v kazalec kapacitete zdravstvenega oz. diagnostičnega sistema.</p>
 <p>Testiranja (skupaj) = Vsota testiranj do dne; podatek je uporaben v smislu primerjave oz. deleža celotne populacije, vendar je zavajajoč, saj so določene osebe lahko testirane večkrat (npr. zdravstveni delavci, zaposleni v DSO...).
</p>
 <p>
Potrjeno okuženi (na dan) = Število potrjeno okuženih na dan. Ta kazalec ne odraža dejanskega gibanja novih okuženih v populaciji, saj s testi ne vzorčijo celotne populacije, ampak se ciljno testira rizične in poklicne skupine.
</p>
 <p>
V intenzivni enoti (trenutno) = Trenutno število oseb v enotah intenzivne terapije.</p>
 <p>Odpuščeni iz bolnišnice (na dan) = Število odpuščenih iz bolnišnice na ta dan.</p>
 <p>Odpuščeni iz bolnišnice (skupaj) = Vsota odpuščenih iz bolnišnice do tega dne.</p>
 <p>Hospitalizirani (trenutno) = Trenutno število oseb v bolnišnični oskrbi (na navadnem oddelku ali enoti za intenzivno terapijo).</p>
<p>
Hospitalizirani (skupaj) = Vsota sprejetih v bolnišnico do dne.
</p>
 <p>Umrli (na dan) = Število umrlih za posledicami COVID-19 na ta dan.
</p>
 <p>Umrli (skupaj) = Vsota umrlih do tega dne.</p>
</div>
</div>

<div class="dropdown">
 <div class="dd-title">Kaj pomeni logaritemska skala na Y osi in kako deluje? 
</div>
 <div class="dd-content">
 <p>Logaritemska skala na navpični osi (ordinata, Y os) je izjemno uporabna za prikaz funkcij oz. količin, ki zelo hitro naraščajo – recimo za t.i. eksponentno rast okuženih –, saj bi v navadnem merilu hitro prerasla najvišjo vrednost na ordinatni osi. </p>
</div>
</div>

<div class="dropdown">
 <div class="dd-title">Kaj pomeni “eksponentna rast okuženih”? Kako lahko merimo, s kakšno hitrostjo se širi epidemija?</div>
 <div class="dd-content">
 <p>Pri epidemijah nalezljivih bolezni je zelo pomembna hitrost širjenja oz. stopnja rasti okužb, saj to vpliva tudi na število obolelih in smrti. Če se število okužb v nekem določenem času povečuje za enako število, npr. za 10 vsake tri dni – 10, 20, 30, 40 ..., gre za linearno rast primerov; če pa se število okužb v določenem časovnem obdobju podvoji, recimo podvojitev za 10 vsake 3 dni – 10, 20, 40, 80 …, pa govorimo o eksponentni rasti, ki v kratkem času privede do zelo velikega števila obolelih.  </p>
 <p>Čas podvojitve kot kazalec hitrosti širjenja epidemije se spreminja (pada, raste), zato ga ne smemo preprosto projicirati v prihodnost; kaže nam zgolj trenutno hitrost podvajanja primerov na podlagi podatkov iz preteklosti.</p>
</div>
</div>

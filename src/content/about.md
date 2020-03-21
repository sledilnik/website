# **Sledenje SLO COVID-19**

Projekt  zbira podatke o širjenju koronavirusa SARS-CoV-2 oz. virusa, ki povzroča COVID-19 v Sloveniji. Vključeni so podatki o številu opravljenih testov, številu potrjeno okuženih v Sloveniji, po posameznih regijah, in občinah. Vodimo tudi evidenco o bolnišnični oskrbi pacientov z COVID-19 (hospitalizirani, intenzivna nega, kritično stanje, odpuščeni iz bolniške oskrbe). Sproti pa se trudimo dodajati tudi nove oz. relevantne kategorije.

Projekt je z zbiranjem podatkov začel [Luka Renko](https://twitter.com/LukaRenko), sedaj pa na njem aktivno dela od 5 do 10 ljudi, saj vnašanje in preverjanje podatkov zahteva vedno več pozornosti. Gre za "crowdsourced" projekt, kjer **lahko vsak prispeva -** tako, da vnese kakšen nov vir ali informacijo iz svojega okolja. Dodate jo enostavno, kot komentar na katerokoli polje v dokumentu, lahko pa preverite tudi  **_Kako urejamo podatke_**  v nadaljevanju. 

## Pokaži mi zbrane podatke  - [GDocs preglednica](https://docs.google.com/spreadsheets/d/1N1qLMoWyi3WFGhIpPFzKsFmVE0IwNP3elb_c18t2DwY/edit#gid=0)   ([CSV izvoz](https://github.com/overlordtm/COVID19.si/tree/master/data)).

### Podatki se že uporabljajo za različne vizualizacije in statistike: 

*   **Better Care** - [Nadzorna plošča Covid 19 SLO](https://app.powerbi.com/view?r=eyJrIjoiMWE2NGNmZWMtMjcxZC00MzkxLWIyMTUtYjExYjI2YTg4NzA0IiwidCI6IjkxMGYyNzY0LWEyZGItNGM2Mi04OGM0LWE1ZTcwYzMzNjVjNCIsImMiOjl9%C2%A0)  ([API](https://bettercare365-my.sharepoint.com/:x:/g/personal/emilp_better_care/EeZA7U_tdFpPjftMy3X2_koBrgpHfQKQvtQMRXPmQakFNw?rtime=eJWxRL3J10g))
*   **Razmerja okužbe/testi** [Matjaž Lipuš - GDocs preglednica](https://docs.google.com/spreadsheets/d/1o9DE8PEXvEOZ0yz02JsUGNhWGx2Q11Ncq2uaY-rE-QY/edit#gid=0)
*   **Zemljevid Slovenije z dnevno rastjo** [Reported COVID-19 cases in the Slovenia](http://milosp.info/maps/interactive/covid19svn/covid19svn.html) (**[Miloš Popovič](https://twitter.com/milos_agathon?lang=en)**) ** 
*   **Primerjalni grafi in zemljevid** [Nace Štruc - Analiza širjenja koronavirusa](https://www.alpaka.si/koronavirus/)

Če bi podatke radi uporabili oz. potrebujete drugače strukturirane podatke za namene podatkovne vizualizacije, predikcijskega modeliranja ali podobno -  kontaktirajte [@LukaRenko](https://twitter.com/lukarenko).

## Zakaj?

**Pravilno zbrani podatki in njihova ažurna ter transparentna objava sta po izkušnjah držav, kjer jim je virus uspelo najbolj zajeziti, kritičnega pomena za učinkovit odziv sistemov javnega zdravja.** Šele tako objavljeni podatki so temelj za razumevanje dogajanja, aktivno samozaščitno ravnanje ljudi ter sprejemanje nujnosti ukrepov. 

NIJZ žal trenutno ne objavlja popolnih in dovolj strukturiranih podatkov o številu okužb, zato jih zbiramo in dopolnjujemo iz različnih javnih virov in jih posredujemo javnosti. Informacije v medijih in uradnih virih, iz katerih črpamo, so pogosto delne, nejasne in nedosledne, zaradi tega preglednica vključuje tudi opombe o virih in spremembah podatkov.  
Zato tudi računamo na vašo pomoč - ali pri zbiranju in preverjanju podatkov, ali pri apeliranju na odločevalce, da ustrezno struktururane podatke objavijo! 

## Kako urejamo podatke:

Statistiko urejamo praviloma ob 14:00 s podatki iz NIJZ. Če so objavljeni vmesni podatki, začasno vnesemo te. Podatki po regijah in starosti se včasih ne ujemajo s tistimi ob 14h (ker niso objavljeni istočasno); dopolnjujemo in preverjamo tudi z podatki, objavljenimi v medijih. 

Kako urejamo bolnišnično oskrbo (Pacienti)

* Spremljamo objave vseh treh bolnišnic za COVID-19 (UKC Ljubljana, UKC Maribor in UK Golnik)
* Spremljamo število hospitaliziranih (vsi oddelki), na intenzivni negi in v kritičnem stanju
* Iz podatkov evidentiramo tudi prehode (sprejem/odpust) med posameznimi stanji.
* Kjer so podatki nepopolni z sklepanjem določimo vrednosti (izvedemo z formulo) 
* Vsi viri in sklepanja so zavedena kot komentar na posameznih celicah

Kako urejamo posamezne primere (Primeri):

*   Posamezne primere spremljamo iz podatkov iz tiskovne konference in objav medijev (link na vir).
*   Zabeležimo predvsem identifikacijske podatke in sledimo izvor in žarišča (kjer je možno).
*   Naredimo placeholderje z minimalno informacij (Izvor - na podlagi podatka po regijah), ki jih v naslednjih dneh dopolnjujemo.
*   Žarišča vodimo iz podatkov o direktnih okužbah in sumarnih podatkov NIJZ (takrat dodamo žarišče na placeholderje).
*   _K:žarišče_, _K:regija_ in _K:kraj_ so kontrolne pivot tabele za validacijo posameznih primerov.

## Bi želel/a sodelovati? 

Če imaš konkretne predlog za izboljšanje strukture ali oblike dokumenta, bi urejal/a vire o zdravstvenem sistemu ali COVID-19, lahko [dobiš pravice za urejanje](javascript:void(location.href='mailto:'+String.fromCharCode(108,117,107,97,46,114,101,110,107,111,64,103,109,97,105,108,46,99,111,109)+'?subject=SLO-Covid-19%20-%20urejanje%2Fpredlog&body=Predlagam%2C%20da...%20')) dokumenta. Če lahko projektu namenite nekaj časa vsak dan, se pridružite [naši Slack skupnosti](http://slo-covid-19.slack.com).  Podatke iz dokumenta lahko prosto uporabljate tudi za lastne  vizualizacije in/ali predikcijske modele - z veseljem jih objavimo na tej strani!

#### Ostale zanimive vsebine

V zavihku [**Viri**](https://docs.google.com/spreadsheets/d/1N1qLMoWyi3WFGhIpPFzKsFmVE0IwNP3elb_c18t2DwY/edit#gid=328677411) si lahko ogledate tudi naše vire podatkov, v zavihku **[Info](https://docs.google.com/spreadsheets/d/1N1qLMoWyi3WFGhIpPFzKsFmVE0IwNP3elb_c18t2DwY/edit#gid=702553602) **pa zbirko povezav na ostale statistike, podatkovne vizualizacije in zanimive objave o metodah in ukrepih, ki se nanašajo na COVID-10 pri nas in po svetu in so vplivale tudi na naše zbiranje podatkov. 

#### Pogoji uporabe

Uporaba in sodelovanje so zaželjeni: podatki so zbrani iz virov v javni domeni in jih lahko prosto uporabljate, urejate, predelujete ali vključujete v vse nekomercialne vsebine ob navedbi vira - [tinyurl.com/slo-covid-19](http://tinyurl.com/slo-covid-19).  
Za izvoz podatkov v drugih formatih ali uporabo za vizualizacije nas kontaktirajte na [@LukaRenko](https://twitter.com/lukarenko)

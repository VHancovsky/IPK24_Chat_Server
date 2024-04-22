# Dokumentácia: IPK24 Chat Server

Author: Viktor Hančovský (xhanco00)

## Obsah
- [Úvod](#úvod)
- [Základné teoretické základy](#základné-teoretické-základy)
   - [Programovanie pomocou socketov](#programovanie-pomocou-socketov)
   - [TCP (Transmission Control Protocol)](#tcp-transmission-control-protocol)
   - [UDP (User Datagram Protocol)](#udp-user-datagram-protocol)
   - [Asynchronne programovanie](#asynchronne-programovanie)
- [Spustenie programu](#spustenie-programu)
   - [Argumenty príkazového riadka](#argumenty-príkazového-riadka)
- [Hlavný program servera](#hlavný-program-servera)
   - [Využité triedy](#využité-triedy)
     - [`Flags`](#flags)
     - [`JoinedClient`](#joinedclient)
     - [`ClientMessage`](#clientmessage)
     - [`Broadcast`](#broadcast)
- [Manuálne testovanie](#manuálne-testovanie)
- [Záver](#záver)
- [Bibliografia](#bibliografia)


## Úvod
IPK24 Server je jednoduchá aplikácia navrhnutá na uskutočňovanie komunikácie medzi viacerými klientmi cez sieť pomocou protokolu TCP. Je určená na poskytovanie základných funkcií, ako je autentifikácia používateľa, výmena správ a pripájanie a odpojovanie sa z kanálov.

## Základné teoretické základy
### Programovanie pomocou socketov
Spôsob komunikácie medzi dvoma počítačmi cez sieť pomocou socketov. Socket je koncový bod pre komunikáciu medzi dvoma programami bežiacimi na rôznych počítačoch alebo na rovnakom počítači. Existujú rôzne typy socketov, ktoré podporujú rôzne typy komunikácie, ako je TCP (Transmission Control Protocol) alebo UDP (User Datagram Protocol).

### TCP (Transmission Control Protocol)
Spojovaný protokol, ktorý zabezpečuje spoľahlivú a poradovú komunikáciu medzi dvoma aplikáciami. V TCP komunikácii je spojenie vytvorené medzi klientom a serverom, pričom dáta sú prenesené v obidvoch smeroch cez toto spojenie. Ide o kontinuálny tok bytov, ktorý umožňuje efektívny prenos dát bez straty poradia.

### UDP (User Datagram Protocol)
Bezspoľahlivý a nespojovaný protokol, ktorý nezaručuje poradie doručenia alebo doručenie dát. Pri UDP komunikácii aplikácia jednoducho posiela dáta na cieľovú adresu a neexistuje žiadne potvrdenie o doručení alebo opätovné odoslanie dát. V našom protokole IPK24-CHAT je navyše správa `CONFIRM` od servera ku každej úspešnej výmene dát, čo zvyšuje spoľahlivosť komunikácie a umožňuje lepšie riadenie stavu spojenia medzi klientom a serverom.

### Asynchronne programovanie
Tento typ programovania umožňuje vykonávať viacero úloh súčasne, čím sa zlepšuje výkonnosť a využitie zdrojov aplikácie. To je obzvlášť užitočné pri socketovom programovaní, kde môže server obsluhovať viacero klientov súčasne bez blokovania.

## Spustenie programu
Pred spustením servera je potrebné zadať konfiguračné parametre prostredníctvom argumentov príkazového riadka.

### Argumenty príkazového riadka
- `-l` <adresa_servera>: Nastaví adresu servera.
- `-p` <port_servra>: Nastaví port servera.
- `-d` <časový_limit> (voliteľné): Časový limit pre UDP potvrdenia.
- `-r` <max_trans> (voliteľné): Maximálny počet prenosov UDP.

## Hlavný program servera
Hlavný program servera je vstupným bodom aplikácie.
Po spustení sa vytvoria sokety pre prijímanie pripojení od klientov a naslúcha na zadanom portu.
Pri každom novom pripojení klienta sa vytvorí nový proces pomocou asynchrónnych úloh, ktorý sa stará o komunikáciu s daným klientom.

### Využité triedy
#### `Flags`
Trieda zodpovedná za parsovanie argumentov príkazového riadku. Obsahuje metódu `ParseFlags`, ktorá analyzuje pole argumentov a nastavuje hodnoty premenných pre adresu servera, port, timeout a maximálny počet prenosov UDP.  hodnoty sa následne používajú na inicializáciu objektu Flags. Trieda teda zohráva dôležitú úlohu pri nastavovaní konfigurácie servera zo vstupných argumentov.

#### `JoinedClient`
Trieda reprezentujúca klienta, ktorý sa pripojil k serveru. Obsahuje vlastnosti ako `UserName`, `DisplayName`, `JoinedChannelName` a `ClientSocket`, ktoré uchovávajú informácie o klientovi a jeho spojení so serverom. Metóda `UserNameInList` slúži na overenie, či je užívateľské meno klienta už obsiahnuté v zozname všetkých pripojených klientov. Táto trieda je dôležitá pre správne identifikovanie a spracovanie informácií o klientoch na serveri.

#### `ClientMessage`
Trieda, ktorá poskytuje metódy na validáciu správ od klientov. Obsahuje metódy ako `ValidateAuth`, `ValidateMsg` a `ValidateJoin`, ktoré overujú syntaktickú správnosť a platnosť správ v rôznych stavoch komunikácie medzi klientom a serverom. Taktiež obsahuje metódu `ChannelInList`, ktorá overuje, či je kanál obsiahnutý v zozname existujúcich kanálov. Trieda `ClientMessage` je dôležitá pre zabezpečenie korektného spracovania správ od klientov na serveri.

#### `Broadcast`
Trieda, ktorá zabezpečuje odosielanie správ klientom. Obsahuje metódu `BroadcastToChannel`, ktorá posiela správy všetkým klientom v určenom kanáli s výnimkou špecifikovaného užívateľa (ak je uvedený). Táto trieda hrá dôležitú úlohu pri zabezpečení správneho rozesielania správ medzi klientmi na serveri.
\
\
Tieto triedy spolu tvoria základnú infraštruktúru servera a zabezpečujú jeho správne fungovanie a komunikáciu s klientmi.

## Záver
Táto dokumentácia poskytuje základné informácie o fungovaní a štruktúre IPK24 Chat serveru, ktoré umožnia správne pochopenie a používanie tohto programu.

## Bibliografia
- [Zadanie druhého projekut](https://git.fit.vutbr.cz/NESFIT/IPK-Projects-2024/src/branch/master/Project%202/iota)
- [UDP wikip0dia](https://en.wikipedia.org/wiki/User_Datagram_Protocol)
- [TCP wikip0dia](https://cs.wikipedia.org/wiki/TCP)
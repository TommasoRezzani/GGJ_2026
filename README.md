# GGJ_2026 — Platform in prima persona con maschere

Prototipo di platform 3D in prima persona realizzato in Unity, organizzato nella repository `GGJ_2026`. Il progetto mette al centro il cambio tra due maschere, **Yuki** e **Oni**, che modificano le possibilità di movimento del giocatore e il suo accompagnamento visivo e sonoro.

Yuki permette un secondo salto in aria; Oni costruisce velocità attraverso una corsa con carica e può ricevere un impulso orizzontale al salto. Il percorso contiene piattaforme, elementi mobili, barriere e un checkpoint: avanzare richiede di scegliere l'abilità adatta e gestire rincorsa, tempismo e traiettoria.

Questo documento descrive gli script, gli asset e le scene presenti nella cartella. Le considerazioni sulle scelte progettuali spiegano gli effetti dell'implementazione osservabile: in assenza di un documento di design, non costituiscono dichiarazioni degli autori sulle loro intenzioni originarie. La verifica è statica; il comportamento completo va confermato nell'Editor.

## Indice

- [Identità e ciclo di gioco](#identità-e-ciclo-di-gioco)
- [Comandi](#comandi)
- [Maschere e movimento](#maschere-e-movimento)
- [Ambiente, ostacoli e checkpoint](#ambiente-ostacoli-e-checkpoint)
- [Presentazione, audio e interfaccia](#presentazione-audio-e-interfaccia)
- [Architettura](#architettura)
- [Tecnologie e struttura della cartella](#tecnologie-e-struttura-della-cartella)
- [Avvio e build](#avvio-e-build)
- [Stato attuale e limiti](#stato-attuale-e-limiti)
- [Crediti e licenze](#crediti-e-licenze)

## Identità e ciclo di gioco

Il nome configurato nelle impostazioni Unity è **GGJ26**. La struttura del progetto è quella di un'esperienza per giocatore singolo, con controllo diretto di un personaggio e camera in prima persona. Nei sorgenti di gameplay non emerge un sistema di combattimento, inventario o progressione a punti: le meccaniche sviluppate riguardano soprattutto l'attraversamento dello spazio.

Il ciclo di gioco è:

1. Avviare la partita dal menu principale.
2. Esaminare il tratto di percorso e selezionare Yuki oppure Oni.
3. Superare le distanze con salti, doppio salto, rincorsa e piattaforme mobili.
4. Attraversare il checkpoint per aggiornare il punto di ritorno.
5. In caso di contatto con una superficie letale, riprendere dall'ultimo checkpoint registrato.

**Scelta di design:** due abilità complementari danno significato alla selezione della maschera. Yuki privilegia la correzione verticale della traiettoria; Oni richiede spazio per accelerare e valorizza la distanza orizzontale. La prima persona lega inoltre la lettura del percorso alla direzione dello sguardo e rende la maschera parte dell'interfaccia del giocatore.

## Comandi

| Comando | Azione |
| --- | --- |
| `W`, `A`, `S`, `D` | Movimento relativo all'orientamento del personaggio |
| Mouse | Rotazione della visuale |
| `Spazio` | Salto; con Yuki, una seconda pressione in aria esegue il doppio salto |
| `Q` | Passaggio ciclico alla maschera successiva |
| `Shift sinistro` + movimento | Corsa con carica di Oni |
| `P` | Apertura/chiusura delle impostazioni durante la partita |
| `Esc` | Apertura/chiusura della conferma di uscita durante la partita |

Il cursore viene bloccato durante il controllo del personaggio e liberato durante la pausa. Il gameplay legge direttamente tastiera e mouse attraverso Unity Input System; la presenza di un file di Input Actions non implica un supporto completo al gamepad o alla rimappatura.

## Maschere e movimento

### Movimento comune

[PlayerCharacterController.cs](GGJ26/Assets/Scripts/PlayerCharacterController.cs) usa un `CharacterController`. Velocità e gravità vengono calcolate esplicitamente e applicate tramite `Move`, senza affidare il movimento principale a un `Rigidbody`.

La direzione diagonale viene normalizzata per evitare un vantaggio di velocità. L'avvicinamento alla velocità desiderata è interpolato con una risposta distinta tra terreno e aria. Lo sguardo ruota il corpo sull'asse orizzontale e la camera su quello verticale, con inclinazione limitata tra −80° e +80°.

I valori serializzati in `MainScene` sono diversi dai valori iniziali dello script:

| Parametro | Valore nella scena principale |
| --- | --- |
| Velocità di base | 7 unità/s |
| Velocità verticale iniziale del salto | 7 unità/s |
| Accelerazione di gravità | −20 unità/s² |
| Risposta del movimento a terra | 10 |
| Risposta del movimento in aria | 2,5 |
| Salti base | 1 |

**Scelta tecnica:** il controller esplicito permette di regolare salti, accelerazione e rimbalzi in modo diretto. La separazione fra risposta a terra e in aria offre un controllo specifico della manovrabilità durante il platforming.

### Yuki: doppio salto

La configurazione [Yuki.asset](GGJ26/Assets/Scripts/Masks/Yuki.asset) assegna il tipo `Yuki` e una forza del secondo salto pari a **10**. Quando il personaggio tocca terra, il controller riabilita il salto aggiuntivo; usandolo in aria, la velocità verticale viene impostata nuovamente a quel valore.

Nella scena principale Yuki è la prima maschera della lista e viene selezionata all'avvio. Non modifica la velocità base né la forza del primo salto, perché i relativi override valgono `-1`.

**Effetto sul design:** il secondo salto consente di recuperare altezza e intervenire sulla traiettoria dopo il distacco dal terreno. Il suono dedicato distingue il salto aggiuntivo da quello iniziale.

### Oni: carica della corsa e salto con slancio

La configurazione [Oni.asset](GGJ26/Assets/Scripts/Masks/Oni.asset) imposta:

| Parametro | Valore |
| --- | --- |
| Ritardo prima dell'accumulo di carica | 0,4 s |
| Incremento del moltiplicatore | 0,75 al secondo |
| Moltiplicatore massimo di velocità | 2,5× |
| Dissipazione del moltiplicatore | 0,5 al secondo |
| Moltiplicatore orizzontale applicato al salto in corsa | 1,5× |
| Soglia relativa per l'interazione del player con i muri | 0,8 del moltiplicatore massimo |

La carica aumenta mantenendo `Shift sinistro` e un input di movimento. Quando questa condizione termina, il moltiplicatore torna progressivamente verso 1. Con la velocità base della scena, la velocità desiderata a piena carica è **17,5 unità/s**. Il salto, se il moltiplicatore supera 1,1, moltiplica la componente orizzontale della velocità; questa resta poi soggetta all'interpolazione del movimento.

**Effetto sul design:** la corsa non produce subito la massima velocità. Il giocatore deve preparare la rincorsa e scegliere il momento del salto. Il termine “dash” compare nei tutorial, ma il codice realizza una corsa progressiva, senza uno scatto autonomo con durata e cooldown.

### Cambio della maschera

[PlayerMaskHandler.cs](GGJ26/Assets/Scripts/PlayerMaskHandler.cs) mantiene una lista di coppie composte da oggetto visivo e configurazione. Al cambio attiva la rappresentazione selezionata, disattiva le altre e passa il nuovo asset al controller.

Le configurazioni sono `ScriptableObject` definiti da [MaskAbilityConfig.cs](GGJ26/Assets/Scripts/MaskAbilityConfig.cs). Il controller ripristina prima le statistiche base, azzerando anche la carica di Oni, poi applica gli override supportati e notifica il cambio all'audio.

**Scelta architetturale:** separare dati e comportamento rende i parametri modificabili dall'Inspector e riutilizzabili. La lista può accogliere altri elementi, ma una nuova abilità richiede anche logica nel controller: il sistema non è un insieme di poteri completamente intercambiabili tramite soli asset.

## Ambiente, ostacoli e checkpoint

### Percorso e piattaforme mobili

La scena principale contiene geometrie realizzate anche con ProBuilder, piattaforme statiche, piattaforme mobili, barriere, un checkpoint e un piano con tag letale. Legno, mattoni e gabbie compongono il repertorio dei materiali ambientali; sono presenti anche una campana e un piano di nebbia.

[MovingPlatformController.cs](GGJ26/Assets/Scripts/MovingPlatformController.cs) sposta una piattaforma tra waypoint in sequenza ciclica, usando `FixedUpdate` e `MoveTowards`. All'avvio separa i waypoint dalla gerarchia della piattaforma, mantenendoli fermi nel mondo. Nella scena principale sono presenti componenti configurati a 4 unità/s.

Il controller del giocatore usa un raggio verso il basso per riconoscere superfici con tag `Floor` e applicarne lo spostamento. Lo script della piattaforma contiene anche una gestione del parenting tramite callback di collisione, la cui applicazione dipende dai componenti fisici utilizzati.

**Scelta tecnica:** i waypoint consentono di costruire percorsi mobili dall'Editor senza creare uno script diverso per ogni piattaforma. Trasferire lo spostamento al giocatore permette al personaggio di accompagnare il supporto durante il tragitto.

### Barriere e rimbalzo

L'interazione è distribuita tra due componenti:

- Il controller del giocatore controlla i trigger con il tag indicato dalla maschera, normalmente `Wall`. Oni supera il controllo relativo se la carica è sufficiente; negli altri casi viene applicato un rimbalzo.
- [FlameWallController.cs](GGJ26/Assets/Scripts/FlameWallController.cs) controlla la velocità orizzontale effettiva e, raggiunta la propria soglia, disabilita collider e renderer e riproduce il suono di successo. Per i tentativi insufficienti è previsto un suono con intervallo minimo di un secondo.

Il rimbalzo azzera la carica, spinge il personaggio lontano dal centro del muro, aggiunge una componente verticale, attiva lo scuotimento della camera e sospende l'input per **0,3 secondi**.

**Effetto sul design:** suono, spostamento e camera comunicano un tentativo fallito attraverso più canali. La riuscita dipende però dalla coerenza delle due verifiche: carica relativa del player e velocità assoluta della barriera non sono la stessa misura.

### Checkpoint e respawn

[PlayerCollisionHandler.cs](GGJ26/Assets/Scripts/PlayerCollisionHandler.cs) registra posizione e rotazione dei trigger con tag `Respawn`. Il contatto con un oggetto `Kill`, tramite trigger o collisione del `CharacterController`, richiama il teletrasporto all'ultimo punto salvato. Prima del primo checkpoint viene usata la posizione iniziale.

Il teletrasporto disabilita temporaneamente il `CharacterController`, riposiziona il personaggio, azzera la velocità e riallinea la visuale. Il checkpoint principale usa anche [Checkpoints.cs](GGJ26/Assets/Scripts/Checkpoints.cs), che definisce la classe `Checkpoint` e riproduce il suono di attivazione una sola volta, se sorgente e clip sono assegnate.

**Effetto sul design:** il ritorno al checkpoint riduce il percorso da ripetere dopo un errore e sostiene tentativi successivi sulle sezioni di movimento. Non è presente un salvataggio persistente dei checkpoint tra sessioni.

## Presentazione, audio e interfaccia

### Direzione visiva

Le maschere Oni e Yuki, le texture dedicate e la campana richiamano un immaginario giapponese. Il progetto combina modelli FBX, materiali con mappe di colore e proprietà di superficie, rappresentazioni delle maschere e overlay di interfaccia. Un filmato MP4 viene utilizzato dal menu attraverso un `VideoPlayer` e una Render Texture.

Sono presenti configurazioni URP per PC e mobile, profili di volume e materiali per la nebbia. Questi asset descrivono la predisposizione del rendering; la loro presenza non certifica una versione mobile pronta o prestazioni misurate su dispositivi specifici. La cartella `Assets/VFX` è attualmente vuota.

### Audio legato alle maschere

[PlayerAudioController.cs](GGJ26/Assets/Scripts/PlayerAudioController.cs) associa a ogni maschera un profilo con musica, salto, doppio salto, passi e corsa. Usa tre sorgenti separate per musica, effetti dei salti e loop di movimento.

Gli eventi `OnJump` e `OnMaskChanged` collegano il gameplay alla riproduzione sonora. I passi si fermano quando il personaggio è in aria o quasi immobile; Oni seleziona il loop di corsa quando il controller segnala lo sprint. Durante la pausa vengono sospesi musica e passi. Per i salti è prevista una piccola variazione casuale del pitch.

La musica del menu è gestita da [BackgroundMusicController.cs](GGJ26/Assets/Scripts/BackgroundMusicController.cs), con istanza unica persistente tra scene; all'avvio della partita il relativo oggetto viene distrutto.

**Scelta progettuale:** cambiare insieme abilità, immagine e paesaggio sonoro rende riconoscibile la maschera selezionata. Gli eventi permettono all'audio di reagire senza inserire la gestione delle clip nel codice del movimento.

### Menu, pausa e preferenze

Il menu espone **Play**, **Settings** e **Quit**, con pannelli per le impostazioni e per la conferma di uscita. [MainMenuController.cs](GGJ26/Assets/Scripts/MainMenuController.cs) avvia la scena successiva nell'ordine di build.

[PauseController.cs](GGJ26/Assets/Scripts/PauseController.cs) gestisce impostazioni e conferma di uscita in partita, libera il cursore e disabilita il controllo del personaggio. La pausa usa `Time.timeScale = 0.0001f`; alla ripresa il valore torna a 1.

Le impostazioni includono:

- **Volume:** [SoundController.cs](GGJ26/Assets/Scripts/SoundController.cs) modifica `AudioListener.volume`. La chiave salvata si chiama `musicVolume`, ma il controllo agisce sul volume globale.
- **Sensibilità del mouse:** [SensitivityController.cs](GGJ26/Assets/Scripts/SensitivityController.cs), che contiene la classe `MouseSensitivityController`, salva `MouseSensitivity` e condivide il valore con il controller della visuale. Il valore predefinito è 0,16.

Entrambi usano `PlayerPrefs`. Non sono previste regolazioni separate di musica ed effetti. I prefab in `Assets/2D/UI/Prefab_Tutorial` forniscono istruzioni in inglese con icone di tastiera per movimento, salto, corsa, cambio maschera e pausa.

## Architettura

| Componente | Responsabilità |
| --- | --- |
| `PlayerCharacterController` | Movimento, visuale, gravità, abilità, rimbalzo, teletrasporto e trasporto sulle piattaforme |
| `PlayerMaskHandler` | Selezione ciclica e attivazione delle maschere |
| `MaskAbilityConfig` | Dati delle abilità e identità della maschera |
| `PlayerCollisionHandler` | Registrazione checkpoint e ritorno dopo contatti letali |
| `Checkpoint` | Attivazione sonora e aggiornamento del punto nel controller |
| `MovingPlatformController` | Movimento ciclico tra waypoint |
| `FlameWallController` | Verifica di velocità della barriera e relativo feedback |
| `PlayerAudioController` | Profili sonori associati alle maschere |
| `CameraShake` | Scuotimento temporaneo e ripristino della camera |
| `MainMenuController`, `PauseController` | Flusso dei pannelli, partita, pausa e uscita |
| `SoundController`, `MouseSensitivityController` | Preferenze locali |
| `BackgroundMusicController` | Ciclo di vita della musica del menu |
| `PlayerRespawn` | Sistema alternativo di ritorno alla posizione iniziale, con reset di un eventuale Rigidbody |
| `PlatformController` | Componente attualmente privo di logica |

La suddivisione distingue dati delle maschere, input e movimento, audio, interfaccia e oggetti ambientali. Parte della logica rimane concentrata nel controller del personaggio; i tag `Player`, `Floor`, `Wall`, `Respawn` e `Kill` costituiscono il collegamento fra oggetti della scena e comportamenti.

## Tecnologie e struttura della cartella

| Tecnologia | Versione locale dichiarata | Utilizzo |
| --- | --- | --- |
| Unity | **6000.3.5f2** | Editor e runtime |
| C# | Versione gestita dall'Editor | Script di gameplay |
| Universal Render Pipeline | 17.3.0 | Rendering |
| Input System | 1.17.0 | Tastiera e mouse |
| ProBuilder | 6.0.8 | Geometrie del livello |
| uGUI | 2.0.0 | Canvas, pulsanti e slider |
| TextMesh Pro | Asset presenti nel progetto | Testi e icone dei tutorial |

Il manifest include anche AI Navigation, Visual Scripting, Multiplayer Center e Test Framework. La loro installazione non dimostra l'uso di navigazione AI, multiplayer o test automatici nel gameplay.

```text
GGJ_2026/
├── README.md
├── .gitignore
├── .gitattributes
├── GGJ26/                       Progetto da aprire con Unity
│   ├── Assets/
│   │   ├── Scripts/              Comportamenti C# e configurazioni delle maschere
│   │   ├── Scenes/               Menu, livello principale e scene di sviluppo
│   │   ├── 3D/                   Modelli, texture e controller di animazione
│   │   ├── 2D/                   Texture, materiali, UI, tutorial e filmato
│   │   ├── Materials/            Materiali di ambiente e oggetti
│   │   ├── Sounds/               Audio di Yuki, Oni, menu e ostacoli
│   │   ├── Settings/             Pipeline di rendering e profili
│   │   ├── ProBuilder Data/      Palette di supporto al level design
│   │   ├── TextMesh Pro/         Risorse di testo e shader
│   │   └── VFX/                  Cartella attualmente vuota
│   ├── Packages/                Manifest e lock file locali
│   └── ProjectSettings/         Configurazione dell'Editor e del gioco
└── enrico_avanzamenti/
    └── New_Project/scenes/       Esportazioni FBX e animazioni delle maschere
```

`enrico_avanzamenti` conserva materiale di lavorazione esterno alla cartella `Assets`: non viene importato automaticamente dal progetto Unity. Le cartelle generate, come `Library`, `Temp` e `Logs`, non sono sorgenti del gioco.

### Scene

| Scena | Ruolo osservabile | Inclusa nella build configurata |
| --- | --- | --- |
| `MainMenu` | Menu, video, musica, impostazioni e conferma uscita | Sì, indice 0 |
| `MainScene` | Percorso principale con player, maschere, ostacoli e UI | Sì, indice 1 |
| `DevScene` | Scena di sviluppo con elementi di player e interfaccia | No |
| `WorldScene` | Scena di prova con piattaforma mobile, ostacoli e barriera | No |
| `SampleScene` | Scena aggiuntiva conservata nel progetto | No |

**Scelta organizzativa:** tenere le scene di sperimentazione fuori dalla build consente di conservare prove e configurazioni senza inserirle nel flusso della partita. L'ordine delle due scene abilitate è funzionale al caricamento per indice usato dal pulsante Play.

### Versionamento

`.gitattributes` tratta i file Unity serializzati come testo e associa le principali estensioni a `unityyamlmerge`; modelli, immagini, audio e video vengono marcati come binari. Il driver di merge deve essere configurato nell'ambiente Git per essere utilizzabile.

`.gitignore` esclude cache, build, log e file locali degli IDE. Contiene anche la regola `/GGJ26/Packages`: nella copia analizzata manifest e lock file sono presenti e risultano già tracciati, ma la regola può nascondere nuovi file non ancora versionati in quella cartella. Per rendere il progetto riproducibile occorre conservare le definizioni dei pacchetti.

## Avvio e build

1. Installare Unity Hub e **Unity 6000.3.5f2**, con il modulo di build per la piattaforma desiderata.
2. Aggiungere a Unity Hub la cartella **`GGJ26`**, non la cartella superiore `GGJ_2026`.
3. Aprire il progetto e attendere importazione degli asset e risoluzione dei pacchetti. La prima apertura può richiedere una connessione per scaricare le dipendenze.
4. Aprire `Assets/Scenes/MainMenu.unity` e avviare Play Mode.
5. Premere **Play** nel menu per caricare `MainScene`. Per verifiche rapide del livello è possibile aprire direttamente la scena principale.
6. Per creare una build, aprire i Build Profiles, selezionare il target e verificare che `MainMenu` preceda `MainScene` nella lista delle scene abilitate.

Le configurazioni di rendering PC/mobile e le dimensioni predefinite delle finestre non equivalgono a build già collaudate. I comandi attuali richiedono tastiera e mouse. `Application.Quit()` produce l'uscita nell'applicazione compilata, mentre non chiude il Play Mode dell'Editor.

Per modificare il bilanciamento, intervenire sui due asset in `Assets/Scripts/Masks` e sui valori del player nella scena. Per aggiungere piattaforme mobili assegnare waypoint validi e una superficie riconoscibile come `Floor`; per checkpoint e pericoli preservare i tag previsti dagli script.

## Stato attuale e limiti

Il contenuto della cartella rappresenta un prototipo con più sistemi integrati e alcune parti ancora da allineare. Le seguenti osservazioni derivano dalla lettura dei file, senza una sessione di gioco eseguita per questo documento.

- **Configurazioni parzialmente applicate.** `ApplyMask` usa gli override di velocità, forza del salto e numero di salti. I campi di gravità, attrito, dash e slide del `MaskAbilityConfig` non vengono applicati dal controller attuale. Anche i flag di abilitazione del secondo salto e del salto con slancio non governano i rispettivi rami: questi dipendono dal tipo di maschera.
- **Tutorial di Yuki da allineare.** Il testo parla di restare sospesi per alcuni secondi, mentre il comportamento implementato è un secondo impulso verticale. Non risulta una meccanica dedicata di planata o sospensione temporizzata.
- **Soglie dei muri distinte.** Le barriere di `MainScene` richiedono 25 unità/s, oltre le 17,5 della corsa desiderata a piena carica. Il boost del salto può portare momentaneamente la velocità orizzontale oltre tale valore. Il controller, invece, verifica la carica relativa e la azzera all'ingresso del trigger: l'ordine e l'interazione delle verifiche richiedono una prova in gioco.
- **Ripristino delle barriere da verificare.** La riattivazione è prevista in `OnTriggerExit`, ma il successo disabilita lo stesso collider. Il codice, da solo, non garantisce il ripristino in ogni situazione.
- **Respawn distribuito.** Il ritorno operativo usa lo stato locale di `PlayerCollisionHandler`; il punto impostato da `Checkpoint` nel controller è uno stato separato, che non viene letto da quel sistema. `PlayerRespawn` offre inoltre un percorso alternativo basato sulla posizione iniziale. Il tag `Respawn` è quindi essenziale per il checkpoint del flusso descritto.
- **Pausa e interfaccia da collaudare insieme.** La pausa rallenta quasi a zero il tempo e blocca il movimento, ma il gestore delle maschere continua a leggere `Q`. Nella scena il pulsante Back delle impostazioni usa chiamate `SetActive`: va verificato che la chiusura visiva sia sempre accompagnata dal ripristino della pausa.
- **Nomi di alcuni script da controllare nell'Editor.** `Checkpoints.cs` contiene `Checkpoint` e `SensitivityController.cs` contiene `MouseSensitivityController`. La corrispondenza tra nome del file e classe del componente va controllata per assicurare il corretto riconoscimento dei `MonoBehaviour`.
- **Persistenza limitata alle preferenze.** Volume e sensibilità sono salvati localmente; non emerge un sistema di salvataggio della partita. Nei sorgenti non è definito un gestore esplicito di vittoria, punteggio, timer o passaggio a ulteriori livelli.
- **Verifica automatica.** Il pacchetto Test Framework è dichiarato, ma nei sorgenti del progetto analizzati non risultano suite di test dedicate. Compilazione, collegamenti dei componenti e giocabilità devono essere verificati nell'Editor.

Questi punti indicano dove concentrare un successivo consolidamento: allineare dati e comportamento delle abilità, unificare le responsabilità di checkpoint e respawn, verificare barriere e pausa e aggiornare le istruzioni a schermo.

## Crediti e licenze

La cartella contiene materiale grafico, sonoro e di lavorazione, ma non un elenco completo e verificabile di autori e ruoli. Il nome `enrico_avanzamenti` identifica una cartella di produzione e non basta per ricostruire i crediti del team.

Non è presente un file di licenza generale nella radice. Le risorse di terze parti mantengono le proprie condizioni: tra i file inclusi sono presenti l'attribuzione EmojiOne e la licenza OFL di Liberation Sans nelle risorse TextMesh Pro. Prima di distribuire il progetto occorre completare i crediti e verificare le condizioni di utilizzo dei singoli asset.

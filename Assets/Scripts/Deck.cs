using UnityEngine;
using UnityEngine.UI;

public class Deck : MonoBehaviour
{
    public Sprite[] faces;
    public GameObject dealer;
    public GameObject player;
    public Button hitButton;
    public Button stickButton;
    public Button playAgainButton;
    public Dropdown creditsButtn;
    public Text playText;
    public Text finalMessage;
    public Text probMessage;
    public Text prob21;
    public Text prob17_21;
    public Text puntosPlayer;
    public Text puntosDealer;
    public Text textoCreditos;

    public int credits = 1000;
    public int apuesta = 0;

    public int[] values = new int[52];
    int cardIndex = 0;    
       
    private void Awake()
    {    
        InitCardValues();        
    }

    private void Start()
    {
        credits = 1000;
        hitButton.interactable = false;
        stickButton.interactable = false;
    }

    private void InitCardValues()
    {
        /*TODO:
         * Asignar un valor a cada una de las 52 cartas del atributo "values".
         * En principio, la posición de cada valor se deberá corresponder con la posición de faces. 
         * Por ejemplo, si en faces[1] hay un 2 de corazones, en values[1] debería haber un 2.
         */

        for (int i = 0; i < values.Length; i++) //Damos valores a cada carta
        {
            switch (i % 13) //El resto
            {
                case 0:
                    values[i] = 11;
                    break;
                case 10:
                case 11:
                case 12:
                    values[i] = 10;
                    break;
                default:
                    values[i] = (i % 13) + 1;
                    break;
            }
        }
    }

    private void ShuffleCards()
    {
        /*TODO:
         * Barajar las cartas aleatoriamente.
         * El método Random.Range(0,n), devuelve un valor entre 0 y n-1
         * Si lo necesitas, puedes definir nuevos arrays.
         */

        //Baraja las cartas
        for (int n = 0; n < values.Length; n++)
        {
            int random = Random.Range(0, 52);
            Sprite caraRandom = faces[random];
            int valorRandom = values[random];

            //Intercambia la posicion actual del bucle al random
            values[random] = values[n];
            faces[random] = faces[n];

            //Intercambia el random a la posicion actual del bucle
            values[n] = valorRandom;
            faces[n] = caraRandom;
        }
    }

    void StartGame()
    {
        apuesta = 0;
        Apostar();
        creditsButtn.interactable = false;
        PushDealer(false);
        PushPlayer(false);
        PushDealer(false);
        PushPlayer(true);

        playAgainButton.interactable = false;
        /*TODO:
            * Si alguno de los dos obtiene Blackjack, termina el juego y mostramos mensaje
        */
            
        if (player.GetComponent<CardHand>().points == 21)
        {
            finalMessage.text = "HAS GANADO";
            hitButton.interactable = false;
            stickButton.interactable = false;
            playText.text = "Play";
            RecibirApuesta("victoria");
            dealer.GetComponent<CardHand>().InitialToggle();
            playAgainButton.interactable = true;
            stickButton.interactable = false;
            hitButton.interactable = false;
        }
        else if (dealer.GetComponent<CardHand>().points == 21)
        {
            finalMessage.text = "HAS PERDIDO";
            playText.text = "Play";
            RecibirApuesta("derrota");
            dealer.GetComponent<CardHand>().InitialToggle();
            playAgainButton.interactable = true;
            stickButton.interactable = false;
            hitButton.interactable = false;
        }
        else if (player.GetComponent<CardHand>().points > 21)
        {
            finalMessage.text = "HAS PERDIDO";
            playText.text = "Play";
            RecibirApuesta("derrota");
            dealer.GetComponent<CardHand>().InitialToggle();
            playAgainButton.interactable = true;
            stickButton.interactable = false;
            hitButton.interactable = false;
        }
    }

    private void CalculateProbabilities()
    {
        /*TODO:
         * Calcular las probabilidades de:
         * - Teniendo la carta oculta, probabilidad de que el dealer tenga más puntuación que el jugador
         * - Probabilidad de que el jugador obtenga entre un 17 y un 21 si pide una carta
         * - Probabilidad de que el jugador obtenga más de 21 si pide una carta          
         */
        int dealerPuntos = dealer.GetComponent<CardHand>().cards[1].GetComponent<CardModel>().value; //Los puntos del dealer sin la carta oculta
        int playerPuntos = player.GetComponent<CardHand>().points;

        int diferencia = Mathf.Abs(dealerPuntos - playerPuntos);
        int contador = 0;

        if (diferencia < 11) //Si la diferencia es mayor a 11 no existe carta para superar al jugador
        {
            for (int i = diferencia + 1; i <= 13; i++) //Sacar el numero de cartas posible que superan al jugador
            {
                contador++;
            }
            if (11 + dealer.GetComponent<CardHand>().cards[1].GetComponent<CardModel>().value < 21) //Para que el as cuente como 11
            {
                contador++;
            }
            if (11 + dealer.GetComponent<CardHand>().cards[1].GetComponent<CardModel>().value > 21 &&
                1 + dealer.GetComponent<CardHand>().cards[1].GetComponent<CardModel>().value > playerPuntos) //Para que el as cuente como 1
            {
                contador++;
            }
            contador = contador * 4; //Como hay 4 palos se multiplica por 4

            for (int i = diferencia + 1; i <= 11; i++) //Resta a las posibles cartas que superan las que ya estan visibles
            {
                for (int j = 1; j < dealer.GetComponent<CardHand>().cards.Count; j++) //Comprueba las del dealer
                {
                    if (dealer.GetComponent<CardHand>().cards[j].GetComponent<CardModel>().value == i + 1) //Si son iguales
                    {
                        contador--;
                    }
                }

                for (int j = 0; j < player.GetComponent<CardHand>().cards.Count; j++) //Comprueba las del player
                {
                    if (player.GetComponent<CardHand>().cards[j].GetComponent<CardModel>().value == i + 1) //Si son iguales
                    {
                        if (i+1 == 11) { //As
                            if (1 + dealerPuntos > playerPuntos)
                            {
                                contador--;
                            }
                        }
                        else //El resto
                        {
                            contador--;
                        }
                    }
                }
            }
        }
        
        float cartasEnLaMesa = (float)dealer.GetComponent<CardHand>().cards.Count + (float)player.GetComponent<CardHand>().cards.Count;
        float cartasDisponibles = (float)values.Length - cartasEnLaMesa;
        float probabilidad = (float)contador/ cartasDisponibles;

        if (probabilidad > 1) //Si se pasa la probabilidad es 1
        {
            probabilidad = 1;
        }

        //Probabilidad de que el jugador obtenga entre un 17 y un 21 si pide una carta
        int contador21 = 0;
        int contador17_21 = 0;
        for(int i=1; i<=13; i++) //Para ver las 13 posibles cartas
        {
            int sumaPuntos = playerPuntos;
            if (i == 1 && playerPuntos<=10) //As 11
            {
                sumaPuntos = sumaPuntos + 11;
            }
            else if (i == 1 && playerPuntos>10) //As 1
            {
                sumaPuntos = sumaPuntos + 1;
            }

            if (i > 1 && i < 11) //Entre 2 y 10 contandolos
            {
                sumaPuntos = sumaPuntos + i;
            }
            if (i > 1 && i >= 11) //Los que valen 11
            {
                sumaPuntos = sumaPuntos + 10;
            }

            if (sumaPuntos > 21)
            {
                contador21++;
            }

            if (sumaPuntos>= 17 && sumaPuntos <= 21)
            {
                contador17_21++;
            }
        }

        probMessage.text = "Deal > Play:   " + probabilidad;
        prob17_21.text = "17<=X<=21:   " + (float)contador17_21 / 13;
        prob21.text = "X > 21:   " + (float)contador21 / 13;
    }

    void PushDealer(bool visible)
    {
        /*TODO:
         * Dependiendo de cómo se implemente ShuffleCards, es posible que haya que cambiar el índice.
        */
        dealer.GetComponent<CardHand>().Push(faces[cardIndex],values[cardIndex], puntosDealer, visible);
        cardIndex++;        
    }

    void PushPlayer(bool calcular)
    {
        /*TODO:
         * Dependiendo de cómo se implemente ShuffleCards, es posible que haya que cambiar el índice.
        */
        player.GetComponent<CardHand>().Push(faces[cardIndex], values[cardIndex], puntosPlayer, true/*,cardCopy*/);
        cardIndex++;

        if (calcular == true)
        {
            CalculateProbabilities();
        }
    }       

    public void Hit()
    {
        /*TODO: 
         * Si estamos en la mano inicial, debemos voltear la primera carta del dealer.
        */

        //Repartimos carta al jugador
        PushPlayer(true);

        /*TODO:
         * Comprobamos si el jugador ya ha perdido y mostramos mensaje
        */
        if (player.GetComponent<CardHand>().points == 21)
        {
            finalMessage.text = "HAS GANADO";
            hitButton.interactable = false;
            stickButton.interactable = false;
            playText.text = "Play";
            dealer.GetComponent<CardHand>().InitialToggle();
            puntosDealer.text = "Puntos:\n" + dealer.GetComponent<CardHand>().points;
            RecibirApuesta("victoria");
            playAgainButton.interactable = true;
            stickButton.interactable = false;
            hitButton.interactable = false;
        }
        else if (dealer.GetComponent<CardHand>().points == 21)
        {
            finalMessage.text = "HAS PERDIDO";
            playText.text = "Play";
            dealer.GetComponent<CardHand>().InitialToggle();
            puntosDealer.text = "Puntos:\n" + dealer.GetComponent<CardHand>().points;
            RecibirApuesta("derrota");
            playAgainButton.interactable = true;
            stickButton.interactable = false;
            hitButton.interactable = false;
        }
        else if (player.GetComponent<CardHand>().points > 21)
        {
            finalMessage.text = "HAS PERDIDO";
            playText.text = "Play";
            dealer.GetComponent<CardHand>().InitialToggle();
            puntosDealer.text = "Puntos:\n" + dealer.GetComponent<CardHand>().points;
            RecibirApuesta("derrota");
            playAgainButton.interactable = true;
            stickButton.interactable = false;
            hitButton.interactable = false;
        }
        CalculateProbabilities();
    }

    public void Stand()
    {
        /*TODO: 
         * Si estamos en la mano inicial, debemos voltear la primera carta del dealer.
         */
        dealer.GetComponent<CardHand>().InitialToggle();
        puntosDealer.text = "Puntos:\n" + dealer.GetComponent<CardHand>().points;
        /*TODO:
         * Repartimos cartas al dealer si tiene 16 puntos o menos
         * El dealer se planta al obtener 17 puntos o más
         * Mostramos el mensaje del que ha ganado
         */
        if (dealer.GetComponent<CardHand>().points <=16)
        {
            PushDealer(true);
        }
        if (dealer.GetComponent<CardHand>().points == 21 ||
            dealer.GetComponent<CardHand>().points > player.GetComponent<CardHand>().points)
        {
            finalMessage.text = "HAS PERDIDO";
            hitButton.interactable = false;
            stickButton.interactable = false;
            playText.text = "Play";
            RecibirApuesta("derrota");
            playAgainButton.interactable = true;
            stickButton.interactable = false;
            hitButton.interactable = false;
        }
        if (dealer.GetComponent<CardHand>().points > 21 ||
            player.GetComponent<CardHand>().points > dealer.GetComponent<CardHand>().points)
        {
            finalMessage.text = "HAS GANADO";
            hitButton.interactable = false;
            stickButton.interactable = false;
            playText.text = "Play";
            RecibirApuesta("victoria");
            playAgainButton.interactable = true;
            stickButton.interactable = false;
            hitButton.interactable = false;
        }
        if (player.GetComponent<CardHand>().points == dealer.GetComponent<CardHand>().points)
        {
            finalMessage.text = "HAS EMPATADO";
            hitButton.interactable = false;
            stickButton.interactable = false;
            playText.text = "Play";
            RecibirApuesta("empate");
            playAgainButton.interactable = true;
            stickButton.interactable = false;
            hitButton.interactable = false;
        }
    }

    public void PlayAgain()
    {
        hitButton.interactable = true;
        stickButton.interactable = true;
        finalMessage.text = "";
        player.GetComponent<CardHand>().Clear();
        dealer.GetComponent<CardHand>().Clear();          
        cardIndex = 0;
        ShuffleCards();
        StartGame();
    }

    public void Apostar()
    {
        switch (creditsButtn.value)
        {
            case 0:
                credits = credits + apuesta;
                credits = credits - 10;
                apuesta = 10;
                textoCreditos.text = "Creditos:\n" + credits;
                break;
            case 1:
                credits = credits + apuesta;
                credits = credits - 20;
                apuesta = 20;
                textoCreditos.text = "Creditos:\n" + credits;
                break;
            case 2:
                credits = credits + apuesta;
                credits = credits - 50;
                apuesta = 50;
                textoCreditos.text = "Creditos:\n" + credits;
                break;
            case 3:
                credits = credits + apuesta;
                credits = credits - 100;
                apuesta = 100;
                textoCreditos.text = "Creditos:\n" + credits;
                break;
        }
    }

    public void RecibirApuesta(string estado)
    {
        if (estado.Equals("empate"))
        {
            credits = credits + apuesta;
        }
        else if (estado.Equals("victoria"))
        {
            credits = credits + (apuesta * 2);
        }
        textoCreditos.text = "Creditos:\n" + credits;
        apuesta = 0;
        creditsButtn.interactable = true;

        if (credits < 100)
        {
            creditsButtn.value = 2;
        }
        if (credits < 50)
        {
            creditsButtn.value = 1;
        }
        if (credits < 20)
        {
            creditsButtn.value = 1;
        }
        if (credits < 10)
        {
            playAgainButton.interactable = false;
        }
    }

    public void Desactivar()
    {
        switch (creditsButtn.value)
        {
            case 0:
                if (credits < 10)
                {
                    playAgainButton.interactable = false;
                }
                else
                {
                    playAgainButton.interactable = true;
                }
                break;
            case 1:
                if (credits < 20)
                {
                    playAgainButton.interactable = false;
                }
                else
                {
                    playAgainButton.interactable = true;
                }
                break;
            case 2:
                if (credits < 50)
                {
                    playAgainButton.interactable = false;
                }
                else
                {
                    playAgainButton.interactable = true;
                }
                break;
            case 3:
                if (credits < 100)
                {
                    playAgainButton.interactable = false;
                }
                else
                {
                    playAgainButton.interactable = true;
                }
                break;
        }
    }
}

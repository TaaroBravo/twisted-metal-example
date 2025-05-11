using System;
using System.Collections;
using UnityEngine;

namespace Corrutinas
{
    public class ExampleCoroutines : MonoBehaviour
    {
        private bool _ready;
        private void Start()
        {
            StopAllCoroutines();
            StartCoroutine(Prueba());
            StartCoroutine(SetearJugadoresListosEn(3));
        }

        private IEnumerator Prueba()
        {
            print("hola");
            yield return new WaitUntil(TodosLosJugadoresEstanListos);
            print("Todos los jugadores estan listos");
        }

        private IEnumerator SetearJugadoresListosEn(int seg)
        {
            yield return new WaitForSeconds(seg);
            yield break;
            _ready = true;
        }

        private bool TodosLosJugadoresEstanListos()
        {
            return _ready;
        }
    }
}
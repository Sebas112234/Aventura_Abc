using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using System.Threading.Tasks;
using TMPro;

public class Inicio_Sesion : MonoBehaviour
{
    [Header("Referencias de Paneles")]
    public GameObject panelLogin;
    public GameObject panelRegistro;

    [Header("Inputs Login")]
    public TMP_InputField emailLogin;
    public TMP_InputField passLogin;

    [Header("Inputs Registro")]
    public TMP_InputField nombreReg;
    public TMP_InputField emailReg;
    public TMP_InputField passReg;

    private FirebaseAuth auth;
    private FirebaseFirestore db;

    void Start()
    {
        // Inicializar Firebase
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;

        // Persistencia: Si el usuario ya inició sesión antes, saltamos al juego
        if (auth.CurrentUser != null)
        {
            Debug.Log("Sesión activa de: " + auth.CurrentUser.Email);
            EntrarAlJuego();
        }
    }

    // --- LÓGICA DE NAVEGACIÓN ---
    public void MostrarRegistro() {
        panelLogin.SetActive(false);
        panelRegistro.SetActive(true);
    }

    public void MostrarLogin() {
        panelLogin.SetActive(true);
        panelRegistro.SetActive(false);
    }

    // --- FUNCIONALIDAD FIREBASE ---

    public void BotonLogin()
    {
        if (string.IsNullOrEmpty(emailLogin.text) || string.IsNullOrEmpty(passLogin.text)) return;

        auth.SignInWithEmailAndPasswordAsync(emailLogin.text, passLogin.text).ContinueWith(task => {
            if (task.IsFaulted) {
                Debug.LogError("Error al iniciar sesión. Revisa tus credenciales.");
                return;
            }
            Debug.Log("Login exitoso!");
            EntrarAlJuego();
        });
    }

    public void BotonRegistrar()
    {
        string nombre = nombreReg.text;
        string email = emailReg.text;
        string pass = passReg.text;

        if (string.IsNullOrEmpty(email) || pass.Length < 6) {
            Debug.Log("Datos inválidos o contraseña muy corta");
            return;
        }

        auth.CreateUserWithEmailAndPasswordAsync(email, pass).ContinueWith(task => {
            if (task.IsFaulted) {
                Debug.LogError("Error en registro: " + task.Exception);
                return;
            }

            FirebaseUser newUser = task.Result.User;
            
            // Creamos el perfil en Firestore
            DocumentReference docRef = db.Collection("Usuarios").Document(newUser.UserId);
            var datosIniciales = new System.Collections.Generic.Dictionary<string, object> {
                { "nombre", nombre },
                { "correo", email },
                { "edad", "" },
                { "progreso", 0.0f }
            };
            
            docRef.SetAsync(datosIniciales).ContinueWith(t => {
                Debug.Log("Usuario registrado y guardado en BD");
                EntrarAlJuego();
            });
        });
    }

    void EntrarAlJuego()
    {
        // UnityEngine.SceneManagement.SceneManager.LoadScene("NombreDeTuEscenaJuego");
        Debug.Log("Cargando Escena de...");
    }
}
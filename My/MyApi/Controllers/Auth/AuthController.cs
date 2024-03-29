﻿using Microsoft.AspNetCore.Mvc;
using MyApi.Models.MyDB;
using System.Data;
using MyApi.Class.Auth;
using MyApi.Models.User;
using MyApi.Controllers.MyTools;

namespace MyApi.Controllers.Auth
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : Controller
    {
       
        public DataTable GetVerifyUser(ValidaUsuario oUsuario)
        {
            /*Declara variables*/
            DataTable dt = new DataTable();

            /*Definiciones*/

            try
            {
                /*Definiciones de las variables*/

                /*Inicia proceso*/
                List<Parametro> parametros = new List<Parametro>{
                    new Parametro("user", oUsuario.usuario)

                };
                dt = DataBase.Listar("sp_proc_ValidaLogin", parametros);

                return dt;

            }
            catch (Exception)
            {
                /*Define return ex*/
                throw;
            }

        }

        
        [HttpPost]
        [Route("VerifySession")]
        public IActionResult VerifySession(ValidaUsuario user)
        {

            /*Declara variables*/
            JsonResult Response;
            bool Code;
            string Message;
            DataTable dtUsuario = new DataTable();
            bool Verify = false;
            string Hash;

            /*Definicion de Variables*/
            try
            {
                /*Inicia proceso*/
                dtUsuario = GetVerifyUser(user);

                if (dtUsuario.Rows.Count > 0)
                {

                    Hash = BCrypt.Net.BCrypt.HashPassword(user.password, dtUsuario.Rows[0]["salt"].ToString());
                    Verify = dtUsuario.Rows[0]["hashpassword"].ToString() == Hash;

                    Code = Verify;
                    Message = Verify ? "Acceso Autorizado" : "Contraseña incorrecta";
                }
                else 
                {
                    Code = false;
                    Message = "Usuario incorrecto";
                }

                if (!Code)
                    dtUsuario.Clear();
                /*Define return */
                
                Response = ToolsController.ToJson(Code, Message,dtUsuario);

            }
            catch (Exception ex)
            {
                /*Define return ex*/
                Code = false;
                Message = "Exception: " + ex;
                Response = ToolsController.ToJson(Code, Message);

            }
            return Response;


        }

        [HttpPost]
        [Route("ChangePassword")]
        public IActionResult ChangePassword(ValidaUsuario Usuarios)
        {
            /*Declara variables*/
            JsonResult Response;
            bool Code;
            string Message;
            DataTable dt;
            string salt;

            try
            {
                /*Inicia proceso*/

                salt = BCrypt.Net.BCrypt.GenerateSalt();

                Usuarios.password = BCrypt.Net.BCrypt.HashPassword(Usuarios.password,salt);

                List<Parametro> parametros = new List<Parametro>{
                    new Parametro("id", Usuarios.id.ToString()),
                    new Parametro("usuario", Usuarios.usuario.ToString()),
                    new Parametro("password", Usuarios.password.ToString()),
                    new Parametro("salt", salt),
                    new Parametro("comentarios", Usuarios.comentarios.ToString()),
                    new Parametro("idUsuarioModifica", Usuarios.idUsuarioModifica.ToString())
                };

                dt = DataBase.Listar("sp_proc_UpdatePassword", parametros);

                /*Define return success*/
                Code = true;
                Message = "Succes";
                Response = ToolsController.ToJson(Code, Message, dt);
            }
            catch (Exception ex)
            {
                /*Define return ex*/
                Code = false;
                Message = "Exception: " + ex;
                Response = ToolsController.ToJson(Code, Message);

            }
            /* Retorna  */
            return Response;

        }
    }
}

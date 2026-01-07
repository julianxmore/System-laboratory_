using Horario_prueba.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Horario_prueba.Models
{
    public class CrearReservaViewModel
    {
        // Estos SÍ son los que vienen del form
        [Required(ErrorMessage = "Selecciona un laboratorio.")]
        public int IdLaboratorio { get; set; }

        [Required(ErrorMessage = "Selecciona un banco.")]
        public int IdBanco { get; set; }

        [Required(ErrorMessage = "Selecciona una franja horaria.")]
        public int IdFranja { get; set; }

        [Required(ErrorMessage = "Selecciona una fecha.")]
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; }

        // Estas son SOLO para llenar los combos, NO se validan
        [ValidateNever]
        public List<Laboratorio> Laboratorios { get; set; } = new();

        [ValidateNever]
        public List<Banco> Bancos { get; set; } = new();

        [ValidateNever]
        public List<FranjaHoraria> Franjas { get; set; } = new();
    }
}




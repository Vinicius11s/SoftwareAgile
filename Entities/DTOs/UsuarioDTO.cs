﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs
{
    public class UsuarioDTO
    {
        public int Id { get; set; }
        public string Login { get; set; } = String.Empty;
        public string Senha { get; set; } = String.Empty;
    }
}

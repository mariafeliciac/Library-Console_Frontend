﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.ModelDto.BookDto
{
    public class BookSearchResponse : BookEditRequest
    {
        public int BookId {  get; set; }
        public int Quantity { get; set; }
    }
}
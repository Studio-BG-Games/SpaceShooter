﻿﻿﻿﻿using DIContainer;

   namespace DIContainer
   {
       public class ResolveSingle<T> where T : class
       {
           private T _value;
           private string _id = "";
           public T Depence => _value ??= DiBox.MainBox.ResolveSingle<T>(_id);

           public ResolveSingle() => _id = "";

           public ResolveSingle(string id) => _id = id;
       }
   }

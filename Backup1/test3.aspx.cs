using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SDK.yop.client;
using System.Reflection;
using SDK.yop.utils;

namespace RsaTest.Styles
{
    public partial class test3 : System.Web.UI.Page
    {
        //非对称RSA签名
        protected void Page_Load(object sender, EventArgs e)
        {
            /*测试私钥*/
            string private_key = "MIIEowIBAAKCAQEAiQKn58QIAazw1/FARY8z374QC72lDQwT0bLn75oKn7c77EDWAvHOIFfvAvDS6s76kqW0ZJ6MwF98dLQ6/Jmb380AVdlIrDSilVN0tkZP8GZJU4lbtxHkPsUJBaD1E6oP/Are5nbp23/X9FAx1yW/CqM+6qcjbnCX7T3XHI3pdxWtnO8rdAs9q3ExbHXCsnaujwGG6AFVuo1Pd2NfvsIzZJ1HEr/GtWS0bSpm35sDDU/vtWmB4rEFoo0h/bt+LccbrjipeHgBK173iQT4bRb+pRrND45WMvwwnEnpzSTNZSTinytlifACDDCZbOCeVNBf2GFZg74cXR8ClN4slt+6sQIDAQABAoIBAESfB+mUzU3JiHcfZclxB9IwJ2k5+simG+cTbAcdZ+TGqUSS1J107oBUimk2pOzl7ao1RDyBDI3tRTBOdJy/csqMqnZU9YkKc8PGoNYKMU9+a4tjaIQwedjWZsZi71yB+K+L4kgbltVRGqBK6iuPxDdXu9NbEuvliLJSHsM2cJArlptDjdps2Ucccpyn8Y3BYTj0Dx/Zm9K19ExCbOQB+hq1cJtTeg+XB77/RmNFnQ7f880/7O4ZiaamKh3x8LgcrrvlIV1SuNaAVRj4Pzm129EU3u2sWa+0wSmIoxqgcf5uJlwvYHtJ/IhaM5RMzAVxW9xs8pqv48Iij/xWtd/mf+ECgYEA0WToYEn0ZrErIHR8wTRuX0F4NXL2AFhPkgq5zL9v6NyyR2MKQM5lxylrsovTWba66pDlMD2u0Qah0vpEldIYNxrK/2MkN6zmzhWsEL9WAjeRiJmoFWPaLLtMTrqOyGhFuOzQja+g7BXjeObNXsUk6gcYm14tsHerKFwrVX0Z4UcCgYEAp4FiYNyjUhx7phSHZsZS0MA8kuCaz0RHSzoyNOudLYs6dkpJuknxdHyH0ZHkfBFpQu8UPlFp18QqEgXttw0mcpQlnxU7p7rLbdLUcT2n3F2u1AICZcbYwjEm77cG+lPI2eCbImRdv+dGO60xYbw9M3LAe7NdZVRGubkdRBzbwEcCgYB74OrpLFd++Ym+Jaw40UAK8ryfQmfvRE+u4tGRJCCc7xQ5z4odVP45mXOxqa9cABMM+rPcmmF2ICpUmuNAj47r6tn1xT2EArJ5cbDeJ5RGs4FSAzXBkdVdaWF3oj8eqTG2ecPYTvMxOLHesQ0G6DMykQgpwsndAK8trXmlleFduQKBgQCPHqGsGVkCg4uyr7+xNKr16grXhElB5xjzUdosFVTK4TcmcvrjzOIdoXl6uqj6yPWjVxt4058X2GgJ1j7yNK4xIBu0/TNncNb4EzepOgD+7JzAUKczxt7VraGTGFNB9+yZHKvwisj/euArhSO4WloAZ3/HMc4PEh8L+Prkg7PPaQKBgBXma5lPNWUPC9MbasCyxVXDCW2emyftRpXUg370suoL5l2h6x41RV+2tOIRzPtOHJpVtC5K7L3KJ2sa7FDVZ+bv6Lf2ct7wTeyVnH/pr7atUXDjM40Oe9RzDESHNfBOValvjerlOFkDpqj5TQmwfIlRZMg2CX9L4hvkA5jSCXB/";
            /*需要签名的原文内容*/
            string originaltext = "merchantNo=10000466938&token=A0CA7BD3625F543925A5DE2D17B53656FDCDFE61413304C7B519A77800E559B5&timestamp=1504849944&directPayType=&cardType=&userNo=liman098&userType=USER_ID&ext=";

            string result = SHA1withRSA.signRsa(originaltext, private_key);

            Response.Write(result);
        }

    }
}